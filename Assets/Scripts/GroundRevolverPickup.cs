using UnityEngine;

public class GroundRevolverPickup : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip itemPickupSound;

    public KeyCode pickupKey = KeyCode.E;

    private bool playerNearby;
    private RobotWeapon playerWeapon;

    private void Awake()
    {
        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer =
                gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite pistolSprite =
            Resources.Load<Sprite>(
                "ElectricPistol"
            );

        if (pistolSprite == null)
        {
            Debug.LogError(
                "ElectricPistol sprite was not found in Assets/Resources."
            );

            return;
        }

        spriteRenderer.sprite =
            pistolSprite;

        spriteRenderer.sortingOrder = 20;
    }

    private void Update()
    {
        if (!playerNearby ||
            playerWeapon == null)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        InteractionMessageUI.SetPrompt(
            this,
            "PICK UP REVOLVER",
            Color.white
        );

        if (Input.GetKeyDown(pickupKey))
            CollectRevolver();
    }

    private void CollectRevolver()
    {
        if (playerWeapon == null)
        {
            Debug.LogWarning(
                "RobotWeapon was not found on the player."
            );

            return;
        }

        int bullets =
            RollBulletCount();

        playerWeapon.PickupRevolver(
            bullets
        );

        InteractionMessageUI.ShowNotification(
            "PICKED UP: REVOLVER",
            Color.cyan
        );

        InteractionMessageUI.ClearPrompt(this);

        Debug.Log(
            "Revolver collected with " +
            bullets +
            " bullets."
        );

        PlayPickupSound();

        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        if (itemPickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                itemPickupSound,
                transform.position,
                1f
            );
        }
    }

    private int RollBulletCount()
    {
        int roll =
            Random.Range(1, 101);

        if (roll <= 20)
            return 8;

        if (roll <= 38)
            return 7;

        if (roll <= 54)
            return 6;

        if (roll <= 68)
            return 5;

        if (roll <= 80)
            return 4;

        if (roll <= 90)
            return 3;

        if (roll <= 98)
            return 2;

        return 1;
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        SetPlayerIfNearby(other);
    }

    private void OnTriggerStay2D(
        Collider2D other
    )
    {
        SetPlayerIfNearby(other);
    }

    private void SetPlayerIfNearby(
        Collider2D other
    )
    {
        RobotWeapon foundWeapon =
            other.GetComponentInParent<
                RobotWeapon
            >();

        if (foundWeapon == null)
            return;

        playerNearby = true;
        playerWeapon = foundWeapon;
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        RobotWeapon foundWeapon =
            other.GetComponentInParent<
                RobotWeapon
            >();

        if (foundWeapon != playerWeapon)
            return;

        playerNearby = false;
        playerWeapon = null;

        InteractionMessageUI.ClearPrompt(this);
    }

    private void OnDisable()
    {
        InteractionMessageUI.ClearPrompt(this);
    }
}
