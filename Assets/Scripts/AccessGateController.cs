using UnityEngine;

public class AccessGateController : MonoBehaviour
{
    [Header("Gate References")]
    public Collider2D gateCollider;
    public SpriteRenderer gatePanel;

    [Header("Gate Identity")]
    public string gateID = "ControlGate";

    [Header("Required Access Card")]
    public string requiredAccessCardID = "ControlCard";

    [Header("Required Key")]
    public bool requiresKey = false;
    public string requiredKeyID;

    [Header("Settings")]
    public float firstScanDuration = 1f;
    public float closeDelay = 0.5f;

    private float scanTimer;
    private bool playerInside;

    private readonly Color lockedColor =
        new Color(0.15f, 0.15f, 0.15f);

    private readonly Color scanningColor =
        new Color(1f, 0.65f, 0f);

    private readonly Color openColor =
        new Color(0f, 1f, 0.2f);

    private void Start()
    {
        CloseGate();
    }

    private bool HasRequiredItem(PlayerInventory inventory)
    {
        if (requiresKey)
            return inventory.HasKey(requiredKeyID);

        return inventory.HasAccessCard(
            requiredAccessCardID
        );
    }

    private void ConsumeRequiredItem(PlayerInventory inventory)
    {
        if (requiresKey)
        {
            inventory.ConsumeKey(requiredKeyID);
        }
        else
        {
            inventory.ConsumeAccessCard(
                requiredAccessCardID
            );
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
            return;

        playerInside = true;

        if (FinalRoomLockState.IsLocked)
        {
            scanTimer = 0f;

            CancelInvoke(nameof(CloseGate));

            ForceCloseGate();

            return;
        }

        if (inventory.IsGateAuthorized(gateID))
        {
            OpenGate();
            return;
        }

        if (!HasRequiredItem(inventory))
        {
            scanTimer = 0f;

            if (gatePanel != null)
                gatePanel.color = lockedColor;

            CloseGate();

            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            scanTimer += Time.deltaTime;

            if (gatePanel != null)
                gatePanel.color = scanningColor;

            if (scanTimer >= firstScanDuration)
            {
                ConsumeRequiredItem(inventory);

                inventory.AuthorizeGate(gateID);

                OpenGate();
            }
        }
        else
        {
            scanTimer = 0f;

            if (gatePanel != null)
                gatePanel.color = lockedColor;

            CloseGate();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
            return;

        playerInside = false;
        scanTimer = 0f;

        CancelInvoke(nameof(CloseGate));

        if (FinalRoomLockState.IsLocked)
        {
            ForceCloseGate();
            return;
        }

        Invoke(
            nameof(CloseGate),
            closeDelay
        );
    }

    private void OpenGate()
    {
        if (FinalRoomLockState.IsLocked)
        {
            ForceCloseGate();
            return;
        }

        if (gatePanel != null)
            gatePanel.color = openColor;

        if (gateCollider != null)
        {
            gateCollider.enabled = false;
            Physics2D.SyncTransforms();
        }
    }

    private void CloseGate()
    {
        if (FinalRoomLockState.IsLocked)
        {
            ForceCloseGate();
            return;
        }

        if (playerInside)
            return;

        if (gatePanel != null)
            gatePanel.color = lockedColor;

        if (gateCollider != null)
        {
            gateCollider.enabled = true;
            Physics2D.SyncTransforms();
        }
    }

    private void ForceCloseGate()
    {
        if (gatePanel != null)
            gatePanel.color = lockedColor;

        if (gateCollider != null)
        {
            gateCollider.enabled = true;
            Physics2D.SyncTransforms();
        }
    }
}