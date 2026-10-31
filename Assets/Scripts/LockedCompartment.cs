using UnityEngine;

public class LockedCompartment : MonoBehaviour
{
    [Header("Required Key")]
    public string requiredKeyID;

    [Header("Facility Task")]
    public string taskID;

    [Header("Sprites")]
    public SpriteRenderer compartmentSpriteRenderer;
    public Sprite beforeUnlockSprite;
    public Sprite afterUnlockSprite;

    [Header("Hidden Component")]
    public GameObject containedComponent;

    [Header("Key Object")]
    public GameObject keyObjectToRemove;

    [Header("Optional Animation")]
    public Animator compartmentAnimator;
    public string openTrigger = "Open";

    private bool playerNearby;
    private bool unlocked;
    private PlayerInventory inventory;

    private void Start()
    {
        unlocked = false;

        if (containedComponent != null)
            containedComponent.SetActive(false);

        ApplySprite(false);
    }

    private void ApplySprite(bool opened)
    {
        if (compartmentSpriteRenderer == null)
            return;

        Sprite selectedSprite =
            opened
                ? afterUnlockSprite
                : beforeUnlockSprite;

        if (selectedSprite != null)
            compartmentSpriteRenderer.sprite = selectedSprite;
    }

    private bool HasRequiredKey()
    {
        if (inventory == null ||
            string.IsNullOrWhiteSpace(requiredKeyID))
        {
            return false;
        }

        return inventory.HasKey(requiredKeyID);
    }

    private void Update()
    {
        if (!playerNearby ||
            unlocked ||
            inventory == null)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        string keyName =
            string.IsNullOrWhiteSpace(requiredKeyID)
                ? "KEY"
                : FormatKeyName(requiredKeyID).ToUpper();

        string message =
            HasRequiredKey()
                ? "UNLOCK COMPARTMENT"
                : keyName + " REQUIRED";

        InteractionMessageUI.SetPrompt(
            this,
            message,
            Color.white
        );

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (HasRequiredKey())
            {
                UnlockCompartment();
            }
            else
            {
                InteractionMessageUI.ShowNotification(
                    keyName + " REQUIRED",
                    Color.yellow
                );
            }
        }
    }

    private void UnlockCompartment()
    {
        if (!HasRequiredKey())
            return;

        unlocked = true;

        InteractionMessageUI.ClearPrompt(this);

        inventory.ConsumeKey(requiredKeyID);

        if (keyObjectToRemove != null)
            keyObjectToRemove.SetActive(false);

        if (containedComponent != null)
            containedComponent.SetActive(true);

        ApplySprite(true);

        if (compartmentAnimator != null &&
            !string.IsNullOrWhiteSpace(openTrigger))
        {
            compartmentAnimator.SetTrigger(openTrigger);
        }

        Debug.Log(
            "Compartment unlocked using key: " +
            requiredKeyID
        );
    }

    private string FormatKeyName(string keyID)
    {
        switch (keyID)
        {
            case "GenALockerKey":
                return "Generator A Locker Key";

            case "StorageLockerKey":
                return "Storage Locker Key";

            case "GeneratorFenceKey":
                return "Generator Fence Key";

            default:
                return keyID;
        }
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

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory != inventory)
            return;

        playerNearby = false;
        inventory = null;

        InteractionMessageUI.ClearPrompt(this);
    }

    private void OnDisable()
    {
        InteractionMessageUI.ClearPrompt(this);
    }
}