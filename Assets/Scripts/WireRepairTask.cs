using UnityEngine;

public class WireRepairTask : MonoBehaviour
{
    [Header("Task")]
    public string taskID;
    public float repairDamage = 20f;
    public float damageCooldown = 0.5f;
    public static bool BlockWeaponInput { get; private set; }

    [Header("Player Input")]
    public GameObject playerRobot;
    private RobotWeapon robotWeapon;

    [Header("Required Wire Set")]
    public string requiredWireSetID;

    [Header("Reward")]
    public GameObject rewardCard;

    [Header("Wire Mode")]
    public bool blackAndWhiteMode = true;
    private static Texture2D circleTexture;

    [Header("Storage Completion")]
    public StorageBlackoutController blackoutController;

    [Header("Panel Appearance")]
    [Tooltip("Image shown before the wires are repaired.")]
    public Sprite beforeRepairSprite;

    [Tooltip("Image shown after the wires are repaired.")]
    public Sprite afterRepairSprite;

    [Header("Disable While Task Is Open")]
    public Behaviour[] disableWhileOpen;

    private bool playerNearby;
    private bool taskComplete;
    private bool taskOpen;

    private PlayerInventory inventory;
    private PlayerHealth playerHealth;
    private RobotSolarRecharge solarRecharge;
    private SpriteRenderer panelRenderer;

    [Header("Wire Settings")]
    [Range(2, 5)]
    public int wireCount = 3;
    private int draggingWire = -1;

    private int[] wireValues;
    private int[] terminalValues;
    private bool[] connectedWires;
    private int[] connectedTerminalForWire;

    private float nextDamageTime;

    private readonly Color[] normalWireColors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta
    };

    private static Texture2D whiteTexture;

    private void Start()
    {
        panelRenderer = GetComponent<SpriteRenderer>();
        ApplyBeforeRepairAppearance();

        if (rewardCard != null)
            rewardCard.SetActive(false);


        if (blackAndWhiteMode && blackoutController == null)
            blackoutController =
                FindAnyObjectByType<StorageBlackoutController>();

        CreateWirePuzzle();
        CreateCircleTexture();

        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }
        
    }

    private void CreateWirePuzzle()
    {
        wireCount = Mathf.Clamp(wireCount, 2, 5);
        wireValues = new int[wireCount];
        terminalValues = new int[wireCount];
        connectedWires = new bool[wireCount];
        connectedTerminalForWire = new int[wireCount];




        for (int i = 0; i < wireCount; i++)
        {
            connectedTerminalForWire[i] = -1;
            wireValues[i] = i;

            terminalValues[i] = wireValues[i];
        }

        ShuffleTerminals();
    }
    private void CreateCircleTexture()
    {
        if (circleTexture != null)
            return;

        int textureSize = 32;

        circleTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        Vector2 center =
            new Vector2(
                textureSize / 2f,
                textureSize / 2f
            );

        float radius =
            textureSize / 2f - 1f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center
                    );

                circleTexture.SetPixel(
                    x,
                    y,
                    distance <= radius
                        ? Color.white
                        : Color.clear
                );
            }
        }

        circleTexture.Apply();
    }

    private void DrawCircle(
        Vector2 center,
        float radius,
        Color color
    )
    {
        if (circleTexture == null)
            CreateCircleTexture();

        Color oldColor = GUI.color;
        GUI.color = color;

        GUI.DrawTexture(
            new Rect(
                center.x - radius,
                center.y - radius,
                radius * 2f,
                radius * 2f
            ),
            circleTexture
        );

        GUI.color = oldColor;
    }

    private void ShuffleTerminals()
    {
        for (int i = 0; i < terminalValues.Length; i++)
        {
            int randomIndex =
                Random.Range(i, terminalValues.Length);

            int temporary = terminalValues[i];
            terminalValues[i] = terminalValues[randomIndex];
            terminalValues[randomIndex] = temporary;
        }
    }

    private void Update()
    {
        if (!playerNearby ||
            inventory == null ||
            taskComplete)
        {
            return;
        }

        if (!TaskIsAvailable())
            return;

        if (solarRecharge != null)
            solarRecharge.NotifyInterrupted();
    }

    private bool TaskIsAvailable()
    {
        if (taskComplete)
            return false;

        if (FacilityTaskManager.Instance == null)
        {
            Debug.LogError(
                "FacilityTaskManager is missing."
            );

            return false;
        }

        return FacilityTaskManager.Instance
            .CanStartTask(taskID);
    }

    private bool HasRequiredWireSet()
    {
        if (string.IsNullOrWhiteSpace(requiredWireSetID))
            return true;

        return inventory != null &&
               inventory.HasSpecificItem(requiredWireSetID);
    }

    private void OpenTask()
    {
        if (!TaskIsAvailable())
        {
            Debug.Log(
                FacilityTaskManager.Instance
                    .GetTaskLockMessage(taskID)
            );

            return;
        }

        if (!HasRequiredWireSet())
            return;

        taskOpen = true;
        draggingWire = -1;

        BlockWeaponInput = true;

        TaskInputBlocker.Block();
        Cursor.visible = true;
    }

    private void CloseTask()
    {
        taskOpen = false;
        draggingWire = -1;

        BlockWeaponInput = false;

        TaskInputBlocker.Unblock();
    }

    private void Awake()
    {
        if (playerRobot != null)
        {
            robotWeapon =
                playerRobot.GetComponentInChildren<RobotWeapon>(
                    true
                );
        }
    }
    private void OnGUI()
    {
        if (!playerNearby || taskComplete)
            return;

        if (!taskOpen)
        {
            DrawOpenTaskPrompt();
            return;
        }

        DrawWirePopup();
    }

    private void DrawOpenTaskPrompt()
    {
        GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
        promptStyle.fontSize = 18;
        promptStyle.alignment = TextAnchor.MiddleCenter;

        bool taskAvailable = TaskIsAvailable();

        if (!taskAvailable)
        {
            string lockMessage =
                FacilityTaskManager.Instance != null
                    ? FacilityTaskManager.Instance
                        .GetTaskLockMessage(taskID)
                    : "Task unavailable.";

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 300f,
                    Screen.height - 150f,
                    600f,
                    45f
                ),
                lockMessage,
                promptStyle
            );

            return;
        }

        string message = HasRequiredWireSet()
            ? "Press the button to repair the wires"
            : "Required wire set missing";

        GUI.Label(
            new Rect(
                Screen.width / 2f - 220f,
                Screen.height - 150f,
                440f,
                35f
            ),
            message,
            promptStyle
        );

        if (!HasRequiredWireSet())
            return;

        if (GUI.Button(
            new Rect(
                Screen.width / 2f - 100f,
                Screen.height - 110f,
                200f,
                50f
            ),
            "OPEN WIRE TASK"
        ))
        {
            OpenTask();
        }
    }

    private void DrawWirePopup()
    {
        float popupWidth =
            Mathf.Min(760f, Screen.width - 40f);

        float popupHeight =
            Mathf.Min(520f, Screen.height - 40f);

        Rect popup = new Rect(
            Screen.width / 2f - popupWidth / 2f,
            Screen.height / 2f - popupHeight / 2f,
            popupWidth,
            popupHeight
        );

        Color oldColor = GUI.color;

        // Dark opaque overlay over the game view.
        GUI.color = new Color(0f, 0f, 0f, 0.85f);

        GUI.DrawTexture(
            new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            ),
            whiteTexture
        );

        // Opaque task window.
        GUI.color = blackAndWhiteMode
            ? new Color(0.82f, 0.82f, 0.82f, 1f)
            : new Color(0.18f, 0.28f, 0.40f, 1f);

        GUI.DrawTexture(popup, whiteTexture);

        GUI.color = Color.white;

        GUIStyle titleStyle =
            new GUIStyle(GUI.skin.label);

        titleStyle.fontSize = 25;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment =
            TextAnchor.MiddleCenter;
        titleStyle.normal.textColor =
            blackAndWhiteMode
                ? Color.black
                : Color.white;

        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 20f,
                popup.width - 80f,
                40f
            ),
            "CONNECT THE WIRES",
            titleStyle
        );

        GUIStyle instructionStyle =
            new GUIStyle(GUI.skin.label);

        instructionStyle.fontSize = 15;
        instructionStyle.alignment =
            TextAnchor.MiddleCenter;
        instructionStyle.normal.textColor =
            blackAndWhiteMode
                ? Color.black
                : Color.white;

        GUI.Label(
            new Rect(
                popup.x + 40f,
                popup.y + 65f,
                popup.width - 80f,
                30f
            ),
            "Drag each wire to its matching terminal",
            instructionStyle
        );

        float leftX =
            popup.x + 115f;

        float rightX =
            popup.x + popup.width - 115f;

        float firstY =
            popup.y + 145f;

        float spacing = 60f;

        Vector2 mousePosition =
            Event.current.mousePosition;

        // Draw visible wire sections before dragging.
        for (int i = 0; i < wireCount; i++)
        {
            float y =
                firstY + i * spacing;

            Vector2 leftStart =
                new Vector2(leftX + 18f, y);

            Vector2 leftEnd =
                new Vector2(
                    popup.x + popup.width / 2f - 25f,
                    y
                );

            Vector2 rightStart =
                new Vector2(
                    popup.x + popup.width / 2f + 25f,
                    y
                );

            Vector2 rightEnd =
                new Vector2(rightX - 18f, y);

            DrawVisibleWire(
                leftStart,
                leftEnd,
                GetDisplayColor(wireValues[i])
            );

            DrawVisibleWire(
                rightStart,
                rightEnd,
                GetDisplayColor(terminalValues[i])
            );
        }

        // Draw completed wires.
        for (int i = 0; i < wireCount; i++)
        {
            if (!connectedWires[i])
                continue;

            int terminalIndex =
                connectedTerminalForWire[i];

            Vector2 start =
                new Vector2(
                    leftX,
                    firstY + i * spacing
                );

            Vector2 end =
                new Vector2(
                    rightX,
                    firstY + terminalIndex * spacing
                );

            DrawVisibleWire(
                start,
                end,
                GetDisplayColor(wireValues[i])
            );
        }

        // Draw the wire currently being dragged.
        if (draggingWire >= 0)
        {
            Vector2 start =
                new Vector2(
                    leftX,
                    firstY + draggingWire * spacing
                );

            DrawVisibleWire(
                start,
                mousePosition,
                GetDisplayColor(wireValues[draggingWire])
            );
        }

        // Draw left-side wire endpoints.
        for (int i = 0; i < wireCount; i++)
        {
            Rect endpoint = new Rect(
                leftX - 18f,
                firstY + i * spacing - 18f,
                36f,
                36f
            );

            DrawEndpoint(
                endpoint,
                GetDisplayColor(wireValues[i]),
                connectedWires[i]
            );

            if (connectedWires[i])
                continue;

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                endpoint.Contains(mousePosition))
            {
                draggingWire = i;
                Event.current.Use();
            }
        }

        // Draw right-side terminal endpoints.
        for (int i = 0; i < wireCount; i++)
        {
            Rect endpoint = new Rect(
                rightX - 18f,
                firstY + i * spacing - 18f,
                36f,
                36f
            );

            bool terminalUsed =
                IsTerminalAlreadyConnected(i);

            DrawEndpoint(
                endpoint,
                GetDisplayColor(terminalValues[i]),
                terminalUsed
            );

            if (draggingWire < 0 ||
                terminalUsed)
            {
                continue;
            }

            if (Event.current.type == EventType.MouseUp &&
                Event.current.button == 0 &&
                endpoint.Contains(mousePosition))
            {
                ConnectTerminal(i);
                Event.current.Use();
            }
        }

        if (Event.current.type == EventType.MouseUp)
            draggingWire = -1;

        // Close button.
        GUI.color = blackAndWhiteMode
            ? Color.black
            : Color.white;

        if (GUI.Button(
            new Rect(
                popup.x + popup.width - 120f,
                popup.y + 20f,
                80f,
                35f
            ),
            "CLOSE"
        ))
        {
            CloseTask();
        }

        GUI.color = oldColor;
    }

    private void DrawVisibleWire(
        Vector2 start,
        Vector2 end,
        Color wireColor
    )
    {
        Color oldColor = GUI.color;

        Color outlineColor =
            new Color(
                0f,
                0f,
                0f,
                0.9f
            );

        // Dark outline.
        DrawLine(
            start,
            end,
            13f,
            outlineColor
        );

        // Main wire.
        DrawLine(
            start,
            end,
            8f,
            wireColor
        );

        // Rounded cylindrical ends.
        DrawCircle(
            start,
            6.5f,
            outlineColor
        );

        DrawCircle(
            end,
            6.5f,
            outlineColor
        );

        DrawCircle(
            start,
            4f,
            wireColor
        );

        DrawCircle(
            end,
            4f,
            wireColor
        );

        GUI.color = oldColor;
    }
    private void DrawEndpoint(
        Rect rect,
        Color endpointColor,
        bool completed
    )
    {
        Vector2 center = rect.center;

        Color oldColor = GUI.color;

        Color outerColor =
            completed
                ? Color.cyan
                : new Color(
                    0.02f,
                    0.04f,
                    0.06f,
                    1f
                );

        Color innerColor =
            completed
                ? Color.white
                : endpointColor;

        // Round outer socket.
        DrawCircle(
            center,
            19f,
            outerColor
        );

        // Colored inner socket.
        DrawCircle(
            center,
            12f,
            innerColor
        );

        // Dark center opening.
        DrawCircle(
            center,
            6f,
            new Color(
                0.01f,
                0.02f,
                0.03f,
                1f
            )
        );

        if (completed)
        {
            GUIStyle checkStyle =
                new GUIStyle(GUI.skin.label);

            checkStyle.fontSize = 16;
            checkStyle.fontStyle =
                FontStyle.Bold;
            checkStyle.alignment =
                TextAnchor.MiddleCenter;
            checkStyle.normal.textColor =
                Color.white;

            GUI.Label(
                new Rect(
                    center.x - 14f,
                    center.y - 14f,
                    28f,
                    28f
                ),
                "✓",
                checkStyle
            );
        }

        GUI.color = oldColor;
    }
    private void DrawLine(
        Vector2 start,
        Vector2 end,
        float width,
        Color color
    )
    {
        Vector2 direction = end - start;
        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;

        GUI.color = color;

        GUIUtility.RotateAroundPivot(angle, start);

        GUI.DrawTexture(
            new Rect(
                start.x,
                start.y - width / 2f,
                direction.magnitude,
                width
            ),
            whiteTexture
        );

        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    private bool IsTerminalAlreadyConnected(int terminalIndex)
    {
        for (int i = 0; i < connectedTerminalForWire.Length; i++)
        {
            if (connectedTerminalForWire[i] == terminalIndex)
                return true;
        }

        return false;
    }

    private void ConnectTerminal(int terminalIndex)
    {
        if (draggingWire < 0)
            return;

        bool correctConnection =
            wireValues[draggingWire] ==
            terminalValues[terminalIndex];

        if (correctConnection)
        {
            connectedWires[draggingWire] = true;
            connectedTerminalForWire[draggingWire] =
                terminalIndex;

            draggingWire = -1;

            if (AllWiresConnected())
                CompleteTask();
        }
        else
        {
            DamagePlayer();
            draggingWire = -1;
        }
    }

    private bool AllWiresConnected()
    {
        for (int i = 0; i < connectedWires.Length; i++)
        {
            if (!connectedWires[i])
                return false;
        }

        return true;
    }

    private void DamagePlayer()
    {
        if (Time.time < nextDamageTime)
            return;

        nextDamageTime =
            Time.time + damageCooldown;

        if (playerHealth != null)
            playerHealth.TakeDamage(repairDamage);

        Debug.Log("Wrong wire connection. Player damaged.");
    }

    private void CompleteTask()
    {
        taskComplete = true;

        CloseTask();

        if (!string.IsNullOrWhiteSpace(requiredWireSetID))
        {
            inventory.ConsumeSpecificItem(
                requiredWireSetID
            );
        }

        if (!string.IsNullOrWhiteSpace(taskID) &&
            FacilityTaskManager.Instance != null)
        {
            FacilityTaskManager.Instance.CompleteTask(taskID);
        }

        if (rewardCard != null)
            rewardCard.SetActive(true);

        if (blackoutController != null)
            blackoutController.RestorePower();

        Debug.Log("Wire task completed: " + taskID);

        ApplyAfterRepairAppearance();
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

        // No colour fallback is needed for a wire task, but this prevents
        // an accidental sprite tint if the completed sprite is not assigned.
    }

    private Color GetDisplayColor(int value)
    {
        Color normalColor =
            normalWireColors[value];

        if (!blackAndWhiteMode)
            return normalColor;

        float grayscale =
            0.299f * normalColor.r +
            0.587f * normalColor.g +
            0.114f * normalColor.b;

        return new Color(
            grayscale,
            grayscale,
            grayscale,
            1f
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DetectPlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DetectPlayer(other);
    }

    private void DetectPlayer(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory == null)
            return;

        if (!playerNearby)
        {
            Debug.Log(
                "WireRepairTask detected player: " +
                other.name
            );
        }

        playerNearby = true;
        inventory = foundInventory;

        playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        solarRecharge =
            other.GetComponentInParent<RobotSolarRecharge>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();
        
        TaskInputBlocker.Unblock();

        if (foundInventory != inventory)
            return;

        playerNearby = false;
        inventory = null;
        playerHealth = null;
        solarRecharge = null;

        if (taskOpen)
            CloseTask();
    }
    private void OnDisable()
    {
        TaskInputBlocker.Unblock();
        if (taskOpen)
            CloseTask();
    }
}
