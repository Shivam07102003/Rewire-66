using UnityEngine;

public class RepairPanel : MonoBehaviour
{
    public enum RequiredComponent
    {
        Fuse,
        SpecificItem,
        CoolantCanister
    }

    [Header("Repair")]
    public float repairDuration = 4f;

    public RequiredComponent requiredComponent =
        RequiredComponent.Fuse;

    [Header("Specific Item")]
    public string requiredItemID;

    [Header("Reward Card")]
    public GameObject rewardCard;
    public GameObject additionalReward;

    [Header("Optional Battery Reward")]
    public BatteryPackSpawner batteryRewardSpawner;

    [Header("Facility Task")]
    public string taskID;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip taskStartSound;
    public AudioClip taskCompleteSound;

    [Header("Panel Appearance")]
    [Tooltip("Image shown before this panel is repaired.")]
    public Sprite beforeRepairSprite;

    [Tooltip("Image shown after this panel is repaired.")]
    public Sprite afterRepairSprite;

    private bool playerNearby;
    private bool repaired;

    private PlayerInventory inventory;
    private SpriteRenderer panelRenderer;

    private float repairProgress;

    public bool IsPlayerNearby
    {
        get
        {
            return playerNearby;
        }
    }

    public bool IsRepairing
    {
        get
        {
            return playerNearby &&
                   inventory != null &&
                   !repaired &&
                   TaskIsAvailable() &&
                   HasRequiredComponent() &&
                   Input.GetKey(KeyCode.E);
        }
    }

    private void Awake()
    {
        panelRenderer =
            GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        ApplyBeforeRepairAppearance();
    }

    private void Start()
    {
        if (rewardCard != null)
            rewardCard.SetActive(false);

        if (additionalReward != null)
            additionalReward.SetActive(false);
    }

    private void Update()
    {
        if (repaired || !playerNearby)
        {
            repairProgress = 0f;
            return;
        }

        if (!TaskIsAvailable())
        {
            repairProgress = 0f;
            return;
        }

        // Required component is missing.
        // Repair cannot begin or continue.
        if (!HasRequiredComponent())
        {
            repairProgress = 0f;
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (repairProgress <= 0f)
                PlayRepairSound(taskStartSound);

            repairProgress += Time.deltaTime;

            if (repairProgress >= repairDuration)
                CompleteRepair();
        }
        else
        {
            repairProgress = 0f;
        }
    }

    private bool TaskIsAvailable()
    {
        if (string.IsNullOrWhiteSpace(taskID))
            return true;

        // Allow prototype tasks even if no manager exists.
        if (FacilityTaskManager.Instance == null)
            return true;

        return FacilityTaskManager.Instance
            .CanStartTask(taskID);
    }

    private bool HasRequiredComponent()
    {
        if (inventory == null)
            return false;

        switch (requiredComponent)
        {
            case RequiredComponent.Fuse:
                return inventory.hasFuse;

            case RequiredComponent.SpecificItem:
                if (string.IsNullOrWhiteSpace(requiredItemID))
                    return false;

                return inventory.HasSpecificItem(
                    requiredItemID
                );

            case RequiredComponent.CoolantCanister:
                return inventory.hasCoolantCanister;
        }

        return false;
    }

    private void ConsumeRequiredComponent()
    {
        if (inventory == null)
            return;

        switch (requiredComponent)
        {
            case RequiredComponent.Fuse:
                inventory.hasFuse = false;
                break;

            case RequiredComponent.SpecificItem:
                inventory.ConsumeSpecificItem(
                    requiredItemID
                );
                break;

            case RequiredComponent.CoolantCanister:
                inventory.hasCoolantCanister = false;
                break;
        }
    }

    private string GetRequiredComponentName()
    {
        switch (requiredComponent)
        {
            case RequiredComponent.Fuse:
                return "Fuse";

            case RequiredComponent.SpecificItem:
                return string.IsNullOrWhiteSpace(requiredItemID)
                    ? "Specific Item"
                    : requiredItemID;

            case RequiredComponent.CoolantCanister:
                return "Coolant Canister";
        }

        return "Component";
    }

    public void InterruptRepair()
    {
        if (repaired)
            return;

        repairProgress = 0f;

        Debug.Log(
            "Repair interrupted by parasite."
        );
    }

    private void CompleteRepair()
    {
        if (repaired)
            return;

        // Safety check so repair can never complete
        // without the required component.
        if (!HasRequiredComponent())
        {
            repairProgress = 0f;
            return;
        }

        repaired = true;
        repairProgress = repairDuration;

        PlayRepairSound(taskCompleteSound);

        ConsumeRequiredComponent();

        if (!string.IsNullOrWhiteSpace(taskID) &&
            FacilityTaskManager.Instance != null)
        {
            FacilityTaskManager.Instance
                .CompleteTask(taskID);
        }

        ApplyAfterRepairAppearance();

        Collider2D panelCollider =
            GetComponent<Collider2D>();

        if (panelCollider != null)
            panelCollider.enabled = false;

        if (rewardCard != null)
            rewardCard.SetActive(true);
        

        if (additionalReward != null)
            additionalReward.SetActive(true);
            
        if (batteryRewardSpawner != null)
        {
            batteryRewardSpawner.gameObject.SetActive(true);
            batteryRewardSpawner.SpawnBatteryPacks();
        }

        Debug.Log(
            GetRequiredComponentName() +
            " repair completed. All rewards revealed."
        );
    }

    private void PlayRepairSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void ApplyBeforeRepairAppearance()
    {
        if (panelRenderer == null)
            return;

        panelRenderer.color = Color.white;

        if (beforeRepairSprite != null)
            panelRenderer.sprite = beforeRepairSprite;
    }

    private void ApplyAfterRepairAppearance()
    {
        if (panelRenderer == null)
            return;

        panelRenderer.color = Color.white;

        if (afterRepairSprite != null)
        {
            panelRenderer.sprite = afterRepairSprite;
            return;
        }

        // Keeps the old green fallback if no completed sprite is assigned.
        panelRenderer.color = Color.green;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory == null)
            return;

        playerNearby = true;
        inventory = foundInventory;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory == null)
            return;

        playerNearby = true;
        inventory = foundInventory;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory != inventory)
            return;

        playerNearby = false;
        inventory = null;
        repairProgress = 0f;
    }
    private string GetRepairDisplayTitle()
    {
        switch (requiredComponent)
        {
            case RequiredComponent.Fuse:
                return "FUSE CONNECTION";

            case RequiredComponent.CoolantCanister:
                return "COOLANT SYSTEM REPAIR";

            case RequiredComponent.SpecificItem:
                string itemID =
                    string.IsNullOrWhiteSpace(requiredItemID)
                        ? ""
                        : requiredItemID.ToLower();

                if (itemID.Contains("circuit") ||
                    itemID.Contains("board"))
                {
                    return "CIRCUIT BOARD REPAIR";
                }

                return GetRequiredComponentName().ToUpper() +
                    " REPAIR";
        }

        return "SYSTEM REPAIR";
    }

    private string GetRepairStatusLine()
    {
        switch (requiredComponent)
        {
            case RequiredComponent.Fuse:
                return "POWER CIRCUIT RESTORATION";

            case RequiredComponent.CoolantCanister:
                return "COOLANT FLOW RESTORATION";

            case RequiredComponent.SpecificItem:
                string itemID =
                    string.IsNullOrWhiteSpace(requiredItemID)
                        ? ""
                        : requiredItemID.ToLower();

                if (itemID.Contains("circuit") ||
                    itemID.Contains("board"))
                {
                    return "CIRCUIT PATH RESTORATION";
                }

                return "COMPONENT SYSTEM RESTORATION";
        }

        return "SYSTEM RESTORATION";
    }
    private void OnGUI()
    {
        if (!playerNearby || repaired)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        if (!TaskIsAvailable())
        {
            InteractionMessageUI.SetPrompt(
                this,
                "COMPLETE PREVIOUS TASK FIRST",
                Color.white
            );

            return;
        }

        if (!HasRequiredComponent())
        {
            InteractionMessageUI.SetPrompt(
                this,
                "REQUIRED: " +
                GetRequiredComponentName().ToUpper(),
                Color.white
            );

            return;
        }

        if (repairProgress <= 0f)
        {
            InteractionMessageUI.SetPrompt(
                this,
                "HOLD TO REPAIR",
                Color.white
            );

            return;
        }

        InteractionMessageUI.SetPrompt(
            this,
            "REPAIRING...",
            Color.cyan
        );

        float progress =
            repairDuration <= 0f
                ? 1f
                : Mathf.Clamp01(
                    repairProgress / repairDuration
                );

        float panelWidth = 360f;
        float panelHeight = 88f;

        Rect panelRect = new Rect(
            Screen.width / 2f - panelWidth / 2f,
            Screen.height - 305f,
            panelWidth,
            panelHeight
        );

        Color oldColor = GUI.color;

        // Outer white border.
        GUI.color = new Color(
            0.82f,
            0.96f,
            1f,
            1f
        );

        GUI.DrawTexture(
            panelRect,
            Texture2D.whiteTexture
        );

        // Blue panel interior.
        Rect innerPanel = new Rect(
            panelRect.x + 2f,
            panelRect.y + 2f,
            panelRect.width - 4f,
            panelRect.height - 4f
        );

        GUI.color = new Color(
            0.02f,
            0.12f,
            0.22f,
            0.98f
        );

        GUI.DrawTexture(
            innerPanel,
            Texture2D.whiteTexture
        );

        // Cyan accent strip.
        GUI.color = Color.cyan;

        GUI.DrawTexture(
            new Rect(
                innerPanel.x,
                innerPanel.y,
                5f,
                innerPanel.height
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        GUIStyle titleStyle =
            new GUIStyle(GUI.skin.label);

        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment =
            TextAnchor.MiddleLeft;
        titleStyle.normal.textColor =
            Color.white;

        GUI.Label(
            new Rect(
                innerPanel.x + 20f,
                innerPanel.y + 8f,
                245f,
                22f
            ),
            GetRepairDisplayTitle(),
            titleStyle
        );

        GUIStyle percentStyle =
            new GUIStyle(GUI.skin.label);

        percentStyle.fontSize = 13;
        percentStyle.fontStyle = FontStyle.Bold;
        percentStyle.alignment =
            TextAnchor.MiddleRight;
        percentStyle.normal.textColor =
            Color.cyan;

        GUI.Label(
            new Rect(
                innerPanel.x + 255f,
                innerPanel.y + 8f,
                80f,
                22f
            ),
            Mathf.RoundToInt(progress * 100f) + "%",
            percentStyle
        );

        Rect barBackground = new Rect(
            innerPanel.x + 20f,
            innerPanel.y + 43f,
            innerPanel.width - 40f,
            18f
        );

        GUI.color = new Color(
            0.08f,
            0.30f,
            0.43f,
            1f
        );

        GUI.DrawTexture(
            barBackground,
            Texture2D.whiteTexture
        );

        Rect barFill = new Rect(
            barBackground.x + 2f,
            barBackground.y + 2f,
            Mathf.Max(
                0f,
                (barBackground.width - 4f) * progress
            ),
            barBackground.height - 4f
        );

        GUI.color = new Color(
            0.00f,
            0.82f,
            1.00f,
            1f
        );

        GUI.DrawTexture(
            barFill,
            Texture2D.whiteTexture
        );

        GUI.color = new Color(
            0.70f,
            0.90f,
            1f,
            1f
        );

        GUIStyle statusStyle =
            new GUIStyle(GUI.skin.label);

        statusStyle.fontSize = 10;
        statusStyle.alignment =
            TextAnchor.MiddleLeft;
        statusStyle.normal.textColor =
            new Color(
                0.70f,
                0.90f,
                1f,
                1f
            );

        GUI.Label(
            new Rect(
                innerPanel.x + 20f,
                innerPanel.y + 65f,
                300f,
                16f
            ),
            GetRepairStatusLine(),
            statusStyle
        );

        GUI.color = oldColor;
    }
}
