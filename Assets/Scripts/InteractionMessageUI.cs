using UnityEngine;

public class InteractionMessageUI : MonoBehaviour
{
    public static InteractionMessageUI Instance;

    private string promptMessage = "";
    private Color promptColor = Color.white;
    private MonoBehaviour promptOwner;
    private float promptEndTime;

    private string notificationMessage = "";
    private Color notificationColor = Color.white;
    private float notificationEndTime;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void SetPrompt(
        MonoBehaviour owner,
        string message,
        Color color
    )
    {
        if (Instance == null)
            return;

        Instance.promptOwner = owner;
        Instance.promptMessage = message;
        Instance.promptColor = color;

        Instance.promptEndTime =
            Time.time + 0.15f;
    }

    public static void ClearPrompt(
        MonoBehaviour owner
    )
    {
        if (Instance == null)
            return;

        if (Instance.promptOwner != owner)
            return;

        Instance.promptOwner = null;
        Instance.promptMessage = "";
    }

    // Compatibility method.
    public static void SetPrompt(
        string message,
        Color color
    )
    {
        if (Instance == null)
            return;

        Instance.promptOwner = null;
        Instance.promptMessage = message;
        Instance.promptColor = color;
    }

    // Compatibility method.
    public static void ClearPrompt()
    {
        if (Instance == null)
            return;

        Instance.promptOwner = null;
        Instance.promptMessage = "";
    }

    public static void ShowNotification(
        string message,
        Color color
    )
    {
        if (Instance == null)
            return;

        Instance.notificationMessage = message;
        Instance.notificationColor = color;
        Instance.notificationEndTime =
            Time.time + 2f;
    }

    private void OnGUI()
    {
        if (
            (
                string.IsNullOrWhiteSpace(promptMessage) ||
                Time.time >= promptEndTime
            ) &&
            Time.time >= notificationEndTime
        )
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(promptMessage) && Time.time < promptEndTime)
        {
            float panelWidth = 540f;
            float panelHeight = 72f;

            Rect panelRect = new Rect(
                Screen.width / 2f -
                    panelWidth / 2f,
                Screen.height - 120f,
                panelWidth,
                panelHeight
            );

            GUI.color =
                new Color(
                    0.01f,
                    0.025f,
                    0.05f,
                    0.96f
                );

            GUI.Box(panelRect, "");

            GUI.color =
                new Color(
                    0f,
                    0.85f,
                    1f,
                    0.95f
                );

            GUI.DrawTexture(
                new Rect(
                    panelRect.x,
                    panelRect.y,
                    4f,
                    panelHeight
                ),
                Texture2D.whiteTexture
            );

            GUI.color = Color.white;

            GUIStyle promptStyle =
                new GUIStyle(
                    GUI.skin.label
                );

            promptStyle.fontSize = 18;
            promptStyle.fontStyle =
                FontStyle.Bold;
            promptStyle.alignment =
                TextAnchor.MiddleCenter;
            promptStyle.normal.textColor =
                promptColor;

            GUI.Label(
                new Rect(
                    panelRect.x + 12f,
                    panelRect.y + 5f,
                    panelWidth - 24f,
                    panelHeight - 10f
                ),
                "[ E ]  " + promptMessage,
                promptStyle
            );
        }

        if (Time.time < notificationEndTime)
        {
            GUIStyle notificationStyle =
                new GUIStyle(
                    GUI.skin.label
                );

            notificationStyle.fontSize = 18;
            notificationStyle.fontStyle =
                FontStyle.Bold;
            notificationStyle.alignment =
                TextAnchor.MiddleCenter;
            notificationStyle.normal.textColor =
                notificationColor;

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 350f,
                    Screen.height - 185f,
                    700f,
                    36f
                ),
                notificationMessage,
                notificationStyle
            );
        }

        GUI.color = Color.white;
    }
} 