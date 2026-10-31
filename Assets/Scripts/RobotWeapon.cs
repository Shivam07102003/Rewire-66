using UnityEngine;
using System.Reflection;
using UnityEngine.EventSystems;
using System.Collections;

public class RobotWeapon : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform weaponPoint;
    public GameObject bulletPrefab;

    [Header("Shooting")]
    public float bulletSpeed = 12f;
    public float bulletLifetime = 2f;
    public float shotsPerSecond = 3f;
    public int damagePerShot = 1;
    public WeaponVisualEffects weaponEffects;

    [Header("Bullet Collision")]
    public LayerMask wallLayers;

    [Header("Ammunition")]
    public int magazineSize = 6;
    public int startingReserveBullets = 0;
    public int maximumReserveBullets = 12;
    public float reloadDuration = 1.2f;

    [Header("Backfire")]
    [Range(0f, 1f)]
    public float backfireChance = 1f / 12f;

    public int backfireDamage = 20;
    public float backfireStunDuration = 2f;

    [Header("Weapon Visual")]
    public GameObject revolverVisual;
    public float weaponHideDelay = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip reloadSound;

    [Header("Aim Indicator")]
    public float aimCircleRadius = 0.65f;
    public float aimLineLength = 1.4f;
    public Color aimColor = Color.cyan;
    public float aimLineWidth = 0.035f;

    public bool startsWithRevolver = false;

    private bool hasRevolver;
    private int reserveBullets;
    private int loadedBullets;
    private Vector3 stunPosition;
    private float nextShotTime;
    private float reloadFinishTime;
    private float stunFinishTime;
    private float lastWeaponUseTime;

    private bool isReloading;
    private bool reloadAfterBackfire;
    private float backfireReloadStartTime;

    private string weaponMessage;
    private float weaponMessageEndTime;

    private LineRenderer aimCircle;
    private LineRenderer aimLine;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        hasRevolver = startsWithRevolver;

        if (weaponEffects == null)
            weaponEffects = GetComponent<WeaponVisualEffects>();
            
        if (hasRevolver)
        {
            reserveBullets =
                Mathf.Clamp(
                    startingReserveBullets,
                    0,
                    maximumReserveBullets
                );

            loadedBullets = magazineSize;
        }
        else
        {
            reserveBullets = 0;
            loadedBullets = 0;
        }

        //CreateAimIndicator();
        HideRevolverVisual();

        if (hasRevolver)
            ShowRevolverVisual();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("LEFT CLICK RECEIVED");
            }
        if (TaskInputBlocker.IsBlocked ||
            RadarFilterUI.IsFilterOpen)
        {
            return;
        }

        UpdateReload();
        UpdateStun();
        UpdateBackfireReload();
        UpdateWeaponVisibility();

        if (Input.GetMouseButtonDown(0))
            StartCoroutine(TryFireAfterUIProcessesClick());

        if (Input.GetKeyDown(KeyCode.R))
            StartReload();
    }
    private IEnumerator TryFireAfterUIProcessesClick()
    {
        yield return null;

        if (TaskInputBlocker.IsBlocked ||
            RadarFilterUI.IsFilterOpen ||
            WireRepairTask.BlockWeaponInput)
        {
            yield break;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            yield break;
        }

        Fire();
    }
    public void Fire()
    {
        if (!hasRevolver)
        {
            ShowMessage("Find a revolver first.");
            return;
        }

        if (IsStunned())
        {
            ShowMessage("STUNNED");
            return;
        }

        if (isReloading)
            return;

        if (loadedBullets <= 0)
        {
            ShowMessage("RELOAD");
            StartReload();
            return;
        }

        if (Time.time < nextShotTime)
            return;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("No camera tagged MainCamera was found.");
            return;
        }

        Vector2 origin =
            weaponPoint != null
                ? weaponPoint.position
                : transform.position;

        Vector2 direction =
            GetAimDirection(origin);

        if (direction.sqrMagnitude <= 0.001f)
            return;

        InterruptNearbyRepairs();

        nextShotTime =
            Time.time +
            1f / Mathf.Max(0.01f, shotsPerSecond);

        ShowRevolverVisual();

        loadedBullets--;

        if (Random.value < backfireChance)
        {
            Backfire();
            return;
        }

        if (weaponEffects != null)
            weaponEffects.PlayFireEffect();

        SpawnBullet(origin, direction);

        ShowMessage("SHOT FIRED");

        Debug.Log(
            "Bullet fired. Reserve: " +
            reserveBullets +
            " | Loaded: " +
            loadedBullets
        );
    }

    private void Backfire()
    {
        loadedBullets = 0;

        ApplyBackfireDamage();

        backfireStunDuration = 2f;
        StartStun();

        if (weaponEffects != null)
            weaponEffects.PlayBackfireEffect();

        isReloading = false;

        reloadAfterBackfire = true;
        backfireReloadStartTime =
            Time.time + backfireStunDuration;

        ShowMessage("SHOT BACKFIRED");

        Debug.Log(
            "SHOT BACKFIRED! Robot lost " +
            backfireDamage +
            " HP and is stunned for 2 seconds."
        );
    }

    private void UpdateBackfireReload()
    {
        if (!reloadAfterBackfire)
            return;

        if (Time.time < backfireReloadStartTime)
            return;

        reloadAfterBackfire = false;

        StartReload();
    }
    private void SpawnBullet(
        Vector2 origin,
        Vector2 direction
    )
    {

        GameObject bullet;

        if (bulletPrefab != null)
        {
            bullet = Instantiate(
                bulletPrefab,
                origin,
                Quaternion.identity
            );
        }
        else
        {
            bullet = CreateFallbackBullet(origin);
        }

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        bullet.transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        BulletProjectile projectile =
            bullet.GetComponent<BulletProjectile>();

        if (projectile == null)
        {
            projectile =
                bullet.AddComponent<BulletProjectile>();
        }

        projectile.direction = direction.normalized;
        projectile.speed = bulletSpeed;
        projectile.damage = damagePerShot;
        projectile.lifetime = bulletLifetime;
        projectile.wallLayers = wallLayers;
    }

    private GameObject CreateFallbackBullet(
        Vector2 position
    )
    {
        GameObject bullet =
            new GameObject("Bullet");

        bullet.transform.position = position;

        SpriteRenderer sprite =
            bullet.AddComponent<SpriteRenderer>();

        Texture2D texture =
            new Texture2D(8, 8);

        Color[] pixels =
            new Color[64];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.yellow;

        texture.SetPixels(pixels);
        texture.Apply();

        sprite.sprite =
            Sprite.Create(
                texture,
                new Rect(0, 0, 8, 8),
                new Vector2(0.5f, 0.5f),
                32f
            );

        sprite.sortingOrder = 100;

        CircleCollider2D collider =
            bullet.AddComponent<CircleCollider2D>();

        collider.isTrigger = true;

        Rigidbody2D body =
            bullet.AddComponent<Rigidbody2D>();

        body.gravityScale = 0f;
        body.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        return bullet;
    }

    public void StartReload()
    {
        if (IsStunned())
            return;
    
        if (!hasRevolver)
            return;

        if (isReloading)
            return;

        if (loadedBullets >= magazineSize)
        {
            ShowMessage("MAGAZINE FULL");
            return;
        }

        if (reserveBullets <= 0)
        {
            ShowMessage("NO RESERVE AMMO");
            return;
        }

        isReloading = true;

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);
            
        reloadFinishTime =
            Time.time + reloadDuration;

        ShowRevolverVisual();
        ShowMessage("RELOADING");
    }

    private void UpdateReload()
    {
        if (!isReloading)
            return;

        if (Time.time < reloadFinishTime)
            return;

        isReloading = false;

        int missingBullets =
            magazineSize - loadedBullets;

        int bulletsToLoad =
            Mathf.Min(
                missingBullets,
                reserveBullets
            );

        loadedBullets += bulletsToLoad;
        reserveBullets -= bulletsToLoad;

        ShowMessage("RELOADED");

        Debug.Log(
            "Reload complete. Reserve: " +
            reserveBullets +
            " | Loaded: " +
            loadedBullets
        );
    }

    public void AddAmmo(int amount)
    {
        if (!hasRevolver)
        {
            ShowMessage("Find a revolver first.");
            return;
        }

        int oldReserve =
            reserveBullets;

        reserveBullets =
            Mathf.Clamp(
                reserveBullets + amount,
                0,
                maximumReserveBullets
            );

        int bulletsAdded =
            reserveBullets - oldReserve;

        if (bulletsAdded > 0)
        {
            ShowMessage(
                "Battery Pack: +" +
                bulletsAdded +
                " bullets"
            );
        }
        else
        {
            ShowMessage("Reserve ammo full.");
        }

        Debug.Log(
            "Battery pack added " +
            bulletsAdded +
            " bullets."
        );
    }

    public void PickupRevolver(int bullets)
    {
        hasRevolver = true;

        // Revolver starts full.
        loadedBullets = magazineSize;

        reserveBullets =
            Mathf.Clamp(
                bullets,
                0,
                maximumReserveBullets
            );

        ShowRevolverVisual();

        ShowMessage("REVOLVER COLLECTED");

        Debug.Log(
            "Revolver collected. Reserve: " +
            reserveBullets +
            " | Loaded: " +
            loadedBullets
        );
    }

    private void ApplyBackfireDamage()
    {
        MonoBehaviour[] components =
            GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component == null ||
                component == this)
            {
                continue;
            }

            string[] damageMethods =
            {
                "TakeDamage",
                "ApplyDamage",
                "Damage",
                "ReceiveDamage"
            };

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            foreach (string methodName in damageMethods)
            {
                MethodInfo method =
                    component.GetType().GetMethod(
                        methodName,
                        flags,
                        null,
                        new[] { typeof(int) },
                        null
                    );

                if (method != null)
                {
                    method.Invoke(
                        component,
                        new object[] { backfireDamage }
                    );

                    return;
                }
            }
        }
    }

    private void StartStun()
    {
        stunFinishTime =
            Time.time + backfireStunDuration;

        // Save the exact position where the shot backfired.
        stunPosition =
            transform.position;

        Rigidbody2D body =
            GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.linearVelocity =
                Vector2.zero;

            body.position =
                stunPosition;
        }
    }

    private void UpdateStun()
    {
        if (!IsStunned())
            return;

        // Stop Rigidbody movement.
        Rigidbody2D body =
            GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.linearVelocity =
                Vector2.zero;

            body.position =
                stunPosition;
        }

        // Stop movement scripts that directly change Transform position.
        transform.position =
            stunPosition;
    }

    public bool IsStunned()
    {
        return Time.time < stunFinishTime;
    }

    private void ShowRevolverVisual()
    {
        lastWeaponUseTime =
            Time.time;

        if (revolverVisual != null)
            revolverVisual.SetActive(true);
    }

    private void HideRevolverVisual()
    {
        if (revolverVisual != null)
            revolverVisual.SetActive(false);
    }

    private void UpdateWeaponVisibility()
    {
        if (revolverVisual == null)
            return;

        if (!hasRevolver)
        {
            HideRevolverVisual();
            return;
        }

        if (Time.time - lastWeaponUseTime >= weaponHideDelay &&
            !isReloading)
        {
            HideRevolverVisual();
        }
    }

    private Vector2 GetAimDirection(
        Vector2 origin
    )
    {
        Vector3 mouseWorld =
            playerCamera.ScreenToWorldPoint(
                new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    -playerCamera.transform.position.z
                )
            );

        return
            ((Vector2)mouseWorld - origin)
            .normalized;
    }

    private void CreateAimIndicator()
    {
        aimCircle =
            CreateLineRenderer("AimCircle");

        aimLine =
            CreateLineRenderer("AimDirection");

        aimCircle.loop = true;
        aimCircle.positionCount = 33;

        aimLine.loop = false;
        aimLine.positionCount = 2;
    }

    private LineRenderer CreateLineRenderer(
        string objectName
    )
    {
        GameObject aimObject =
            new GameObject(objectName);

        aimObject.transform.SetParent(transform);

        LineRenderer line =
            aimObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.startWidth = aimLineWidth;
        line.endWidth = aimLineWidth;
        line.startColor = aimColor;
        line.endColor = aimColor;

        line.material =
            new Material(
                Shader.Find("Sprites/Default")
            );

        line.sortingOrder = 50;

        return line;
    }

    private void UpdateAimIndicator()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Vector2 center =
            transform.position;

        for (int i = 0; i < aimCircle.positionCount; i++)
        {
            float angle =
                i * Mathf.PI * 2f /
                (aimCircle.positionCount - 1);

            Vector2 point =
                center +
                new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ) *
                aimCircleRadius;

            aimCircle.SetPosition(i, point);
        }

        Vector2 origin =
            weaponPoint != null
                ? weaponPoint.position
                : transform.position;

        Vector2 direction =
            GetAimDirection(origin);

        aimLine.SetPosition(0, origin);

        aimLine.SetPosition(
            1,
            origin +
            direction * aimLineLength
        );
    }

    private void LateUpdate()
    {
        if (!IsStunned())
            return;

        // LateUpdate runs after the movement script.
        // This keeps the robot exactly where the shot backfired.
        transform.position =
            stunPosition;

        Rigidbody2D body =
            GetComponent<Rigidbody2D>();

        if (body != null)
        {
            body.linearVelocity =
                Vector2.zero;

            body.position =
                stunPosition;
        }
    }

    private void InterruptNearbyRepairs()
    {
        RepairPanel[] panels =
            FindObjectsByType<RepairPanel>();

        foreach (RepairPanel panel in panels)
        {
            if (panel != null &&
                panel.IsPlayerNearby)
            {
                panel.InterruptRepair();
            }
        }
    }
    private void ShowMessage(string text)
    {
        weaponMessage = text;
        weaponMessageEndTime =
            Time.time + 2f;
    }

    public string GetWeaponMessage()
    {
        if (Time.time > weaponMessageEndTime)
            return "";

        string currentMessage =
            weaponMessage;

        weaponMessage = "";

        return currentMessage;
    }
    public int GetMaximumReserveBullets()
    {
        return maximumReserveBullets;
    }

    public bool HasRevolver()
    {
        return hasRevolver;
    }

    public int GetReserveBullets()
    {
        return reserveBullets;
    }

    public int GetLoadedBullets()
    {
        return loadedBullets;
    }

    public int GetShotsRemaining()
    {
        return loadedBullets;
    }

    public int GetMaximumShots()
    {
        return magazineSize;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}