using UnityEngine;

public class HiddenComponentSearch : MonoBehaviour
{
    [Header("Item")]
    public string componentName = "Fuse";

    [Header("Specific Item")]
    public string itemID;

    [Header("Access Card")]
    public string accessCardID;

    [Header("Search Distance")]
    public float revealDistance = 0.8f;
    public float collectDistance = 0.3f;

    [Header("Audio")]
    public AudioClip revealSound;
    public AudioClip pickupSound;

    private Transform player;
    private PlayerInventory inventory;
    private SpriteRenderer componentRenderer;
    private bool revealed;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            inventory =
                playerObject.GetComponentInParent<PlayerInventory>();
        }

        componentRenderer =
            GetComponent<SpriteRenderer>();

        if (componentRenderer != null)
            componentRenderer.enabled = false;
    }

    private void Update()
    {
        if (player == null ||
            inventory == null)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        float distance =
            Vector2.Distance(
                player.position,
                transform.position
            );

        if (!revealed &&
            distance <= revealDistance)
        {
            RevealComponent();
        }

        if (!revealed)
        {
            InteractionMessageUI.ClearPrompt(this);
            return;
        }

        if (distance <= collectDistance)
        {
            InteractionMessageUI.SetPrompt(
                this,
                "PICK UP " +
                componentName.ToUpper(),
                Color.white
            );

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (CollectComponent())
                {
                    InteractionMessageUI.ShowNotification(
                        "PICKED UP: " +
                        componentName.ToUpper(),
                        Color.cyan
                    );

                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(
                            pickupSound,
                            transform.position
                        );
                    }

                    InteractionMessageUI.ClearPrompt(this);

                    Debug.Log(
                        componentName +
                        " automatically collected."
                    );

                    gameObject.SetActive(false);
                }
            }
        }
        else
        {
            InteractionMessageUI.ClearPrompt(this);
        }
    }

    private void RevealComponent()
    {
        revealed = true;

        if (componentRenderer != null)
            componentRenderer.enabled = true;

        if (revealSound != null)
        {
            AudioSource.PlayClipAtPoint(
                revealSound,
                transform.position
            );
        }

        Debug.Log(
            "Component discovered: " +
            componentName
        );
    }

    private bool CollectComponent()
    {
        if (componentName == "Fuse")
        {
            inventory.AddFuse();
            return true;
        }

        if (componentName == "Coolant Canister")
        {
            inventory.AddCoolantCanister();
            return true;
        }

        if (componentName == "Circuit Board" ||
            componentName == "Wire Set" ||
            componentName == "Specific Item")
        {
            if (string.IsNullOrWhiteSpace(itemID))
            {
                Debug.LogWarning(
                    gameObject.name +
                    " has no Item ID."
                );

                return false;
            }

            inventory.AddSpecificItem(itemID);
            return true;
        }

        if (componentName == "Access Card")
        {
            if (string.IsNullOrWhiteSpace(accessCardID))
            {
                Debug.LogWarning(
                    gameObject.name +
                    " has no Access Card ID."
                );

                return false;
            }

            inventory.AddAccessCard(accessCardID);
            return true;
        }

        Debug.LogWarning(
            "Unknown hidden component type: " +
            componentName
        );

        return false;
    }
        private void OnDisable()
    {
        InteractionMessageUI.ClearPrompt(this);
    }
}