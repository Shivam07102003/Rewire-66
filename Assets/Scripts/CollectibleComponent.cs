using TMPro;
using UnityEngine;

public class CollectibleComponent : MonoBehaviour
{
    [Header("Item")]
    public string componentName = "Fuse";

    [Header("Specific Item Settings")]
    public string itemID;

    [Header("Key Settings")]
    public string keyID;

    [Header("Access Card Settings")]
    public string accessCardID = "ControlCard";
    public string cardLabel = "CTRL";

    [Header("World Text")]
    public TMP_Text labelText;

    private bool playerNearby;
    private PlayerInventory inventory;

    private void Start()
    {
        if (componentName == "Access Card")
        {
            if (labelText == null)
                labelText = GetComponentInChildren<TMP_Text>();

            if (labelText != null)
                labelText.text = cardLabel;
        }
    }

    private void Update()
    {
        if (!playerNearby || inventory == null)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        InteractionMessageUI.SetPrompt(
            this,
            "PICK UP " + GetPickupName().ToUpper(),
            Color.white
        );

        if (Input.GetKeyDown(KeyCode.E))
            CollectComponent();
    }

    private void CollectComponent()
    {
        string pickedUpName = GetPickupName();

        if (componentName == "Fuse")
        {
            inventory.AddFuse();
        }
        else if (componentName == "Coolant Canister")
        {
            inventory.AddCoolantCanister();
        }
        else if (componentName == "Circuit Board" ||
                 componentName == "Wire Set" ||
                 componentName == "Specific Item")
        {
            if (string.IsNullOrWhiteSpace(itemID))
                return;

            inventory.AddSpecificItem(itemID);
        }
        else if (componentName == "Key")
        {
            if (string.IsNullOrWhiteSpace(keyID))
                return;

            inventory.AddKey(keyID);
        }
        else if (componentName == "Access Card")
        {
            if (string.IsNullOrWhiteSpace(accessCardID))
                return;

            inventory.AddAccessCard(accessCardID);
        }
        else
        {
            return;
        }

        InteractionMessageUI.ShowNotification(
            "PICKED UP: " + pickedUpName.ToUpper(),
            Color.cyan
        );

        InteractionMessageUI.ClearPrompt(this);

        Debug.Log("Collected: " + pickedUpName);

        gameObject.SetActive(false);
    }

    private string GetPickupName()
    {
        return componentName;
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