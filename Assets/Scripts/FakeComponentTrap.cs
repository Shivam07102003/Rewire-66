using System.Collections.Generic;
using UnityEngine;

public class FakeComponentTrap : MonoBehaviour
{
    public enum FakeComponentType
    {
        FakeFuse,
        FakeCircuitBoard,
        FakeCoolantCanister,
        FakeBatteryPack,
        FakeAccessCard,
        FakeLockerKey,
        Custom
    }

    [Header("Fake Component")]
    public FakeComponentType componentType =
        FakeComponentType.FakeFuse;

    [Tooltip("Used only when Component Type is Custom.")]
    public string customComponentName =
        "Fake Component";

    [Header("Trap")]
    public GameObject parasitePrefab;
    public Transform[] spawnPoints;

    [Min(0f)]
    public float alarmDuration = 3f;

    [Header("Audio")]
    public AudioClip warningSound;

    [Header("Collection")]
    public KeyCode collectKey = KeyCode.E;
    public bool destroySpawnPointsAfterCollection = true;

    private bool playerNearby;
    private bool trapTriggered;
    private bool collected;
    private float alarmEndTime;

    private RadarTarget radarTarget;

    private readonly List<GameObject> spawnedParasites =
        new List<GameObject>();

    public List<GameObject> SpawnedParasites
    {
        get
        {
            return spawnedParasites;
        }
    }

    private void Awake()
    {
        radarTarget =
            GetComponent<RadarTarget>();
    }

    private void Update()
    {
        if (!playerNearby ||
            trapTriggered ||
            collected)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        InteractionMessageUI.SetPrompt(
            this,
            GetCollectionMessage().ToUpper(),
            Color.white
        );

        if (Input.GetKeyDown(collectKey))
            CollectFakeComponent();
    }

    private void CollectFakeComponent()
    {
        collected = true;
        trapTriggered = true;

        alarmEndTime =
            Time.time + alarmDuration;

        PlayWarningSound();

        Debug.Log(
            "Collected fake component: " +
            GetComponentName()
        );

        if (radarTarget != null)
            radarTarget.showOnRadar = false;

        SpawnParasites();

        if (destroySpawnPointsAfterCollection)
            DeleteSpawnPoints();

        Collider2D ownCollider =
            GetComponent<Collider2D>();

        if (ownCollider != null)
            ownCollider.enabled = false;

        gameObject.SetActive(false);
    }

    public List<GameObject> TriggerFromExternalSource()
    {
        if (trapTriggered)
            return spawnedParasites;

        collected = true;
        trapTriggered = true;

        // The communication tower will display
        // its own warning message.
        alarmEndTime = 0f;

        PlayWarningSound();

        Debug.Log(
            "External trap triggered: " +
            GetComponentName()
        );

        SpawnParasites();

        return spawnedParasites;
    }

    private void SpawnParasites()
    {
        spawnedParasites.Clear();

        if (parasitePrefab == null)
        {
            Debug.LogWarning(
                "FakeComponentTrap: " +
                "Parasite Prefab is not assigned."
            );

            return;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "FakeComponentTrap: " +
                "No spawn points are assigned."
            );

            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform spawnPoint =
                spawnPoints[i];

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    "FakeComponentTrap: " +
                    "Spawn point " +
                    i +
                    " is empty."
                );

                continue;
            }

            GameObject spawnedParasite =
                Instantiate(
                    parasitePrefab,
                    spawnPoint.position,
                    Quaternion.identity
                );

            spawnedParasites.Add(
                spawnedParasite
            );

            Debug.Log(
                "Parasite spawned at: " +
                spawnedParasite.transform.position
            );
        }
    }

    private void DeleteSpawnPoints()
    {
        if (spawnPoints == null)
            return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                Destroy(
                    spawnPoints[i].gameObject
                );
        }

        spawnPoints = null;
    }

    private void PlayWarningSound()
    {
        if (warningSound == null)
            return;

        Vector3 soundPosition =
            Camera.main != null
                ? Camera.main.transform.position
                : transform.position;

        AudioSource.PlayClipAtPoint(
            warningSound,
            soundPosition,
            1f
        );
    }

    private string GetComponentName()
    {
        switch (componentType)
        {
            case FakeComponentType.FakeFuse:
                return "Fake Fuse";

            case FakeComponentType.FakeCircuitBoard:
                return "Fake Circuit Board";

            case FakeComponentType.FakeCoolantCanister:
                return "Fake Coolant Canister";

            case FakeComponentType.FakeBatteryPack:
                return "Fake Battery Pack";

            case FakeComponentType.FakeAccessCard:
                return "Fake Access Card";

            case FakeComponentType.FakeLockerKey:
                return "Fake Locker Key";

            case FakeComponentType.Custom:
                return string.IsNullOrWhiteSpace(
                    customComponentName
                )
                    ? "Fake Component"
                    : customComponentName;
        }

        return "Fake Component";
    }

    private string GetCollectionMessage()
    {
        switch (componentType)
        {
            case FakeComponentType.FakeFuse:
                return "PICK UP FUSE";

            case FakeComponentType.FakeCircuitBoard:
                return "PICK UP CIRCUIT BOARD";

            case FakeComponentType.FakeCoolantCanister:
                return "PICK UP COOLANT CANISTER";

            case FakeComponentType.FakeBatteryPack:
                return "PICK UP BATTERY PACK";

            case FakeComponentType.FakeAccessCard:
                return "PICK UP ACCESS CARD";

            case FakeComponentType.FakeLockerKey:
                return "PICK UP LOCKER KEY";

            case FakeComponentType.Custom:
                return "PICK UP " + GetComponentName();
        }

        return "PICK UP";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") ||
            other.GetComponentInParent<PlayerInventory>() != null)
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        if (other.CompareTag("Player") ||
            other.GetComponentInParent<
                PlayerInventory
            >() != null)
        {
            playerNearby = false;

            InteractionMessageUI.ClearPrompt(
                this
            );
        }
    }
    private void OnDisable()
    {
        InteractionMessageUI.ClearPrompt(this);
    }
    private void OnGUI()
    {
        if (Time.time >= alarmEndTime)
            return;

        GUIStyle alarmStyle =
            new GUIStyle(GUI.skin.label);

        alarmStyle.fontSize = 28;
        alarmStyle.fontStyle =
            FontStyle.Bold;
        alarmStyle.alignment =
            TextAnchor.MiddleCenter;
        alarmStyle.normal.textColor =
            Color.red;

        GUI.Label(
            new Rect(
                Screen.width / 2f - 300f,
                40f,
                600f,
                50f
            ),
            "WARNING: FAKE COMPONENT — " +
            GetComponentName().ToUpper(),
            alarmStyle
        );
    }
}
