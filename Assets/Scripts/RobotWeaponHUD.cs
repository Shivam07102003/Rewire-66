using UnityEngine;

public class RobotWeaponHUD : MonoBehaviour
{
    [Header("References")]
    public RobotWeapon weapon;

    [Header("Position")]
    public Vector2 margin = new Vector2(24f, 24f);
    public float panelWidth = 100f;
    public float panelHeight = 44f;

    [Header("Appearance")]
    public Color panelColor = new Color(0.02f, 0.05f, 0.10f, 0.92f);
    public Color normalColor = Color.white;
    public Color messageColor = Color.yellow;

    private Texture2D whitePixel;
    private string message = "";
    private float messageEndTime;

    private void Awake()
    {
        if (weapon == null)
            weapon = GetComponent<RobotWeapon>();

        whitePixel = new Texture2D(1, 1);
        whitePixel.SetPixel(0, 0, Color.white);
        whitePixel.Apply();
    }

    private void Update()
    {
        if (weapon == null)
            return;

        string weaponMessage = weapon.GetWeaponMessage();

        if (!string.IsNullOrEmpty(weaponMessage))
        {
            message = weaponMessage;
            messageEndTime = Time.time + 2f;
        }
    }

    private void OnGUI()
    {
        if (weapon == null)
            return;

        Rect panelRect = new Rect(
            Screen.width - panelWidth - margin.x,
            Screen.height - panelHeight - margin.y,
            panelWidth,
            panelHeight
        );

        Color oldColor = GUI.color;

        GUI.color = panelColor;

        GUI.DrawTexture(
            panelRect,
            whitePixel
        );

        GUI.color =
            new Color(0f, 0.85f, 1f, 0.95f);

        GUI.DrawTexture(
            new Rect(
                panelRect.x,
                panelRect.y,
                panelRect.width,
                3f
            ),
            whitePixel
        );

        GUI.color = Color.white;

        GUIStyle ammoStyle =
            new GUIStyle(GUI.skin.label);

        ammoStyle.fontSize = 19;
        ammoStyle.fontStyle =
            FontStyle.Bold;
        ammoStyle.alignment =
            TextAnchor.MiddleCenter;
        ammoStyle.normal.textColor =
            normalColor;

        string ammoText;

        if (weapon.HasRevolver())
        {
            ammoText =
                "AMMO  " +
                weapon.GetLoadedBullets() +
                " / " +
                weapon.GetReserveBullets();
        }
        else
        {
            ammoText = "AMMO  —  LOCKED";
        }

        GUI.Label(
            panelRect,
            ammoText,
            ammoStyle
        );

        if (Time.time < messageEndTime)
        {
            GUIStyle messageStyle =
                new GUIStyle(GUI.skin.label);

            messageStyle.fontSize = 17;
            messageStyle.fontStyle =
                FontStyle.Bold;
            messageStyle.alignment =
                TextAnchor.MiddleCenter;
            messageStyle.normal.textColor =
                messageColor;

            GUI.Label(
                new Rect(
                    panelRect.x - 100f,
                    panelRect.y - 42f,
                    panelRect.width + 200f,
                    32f
                ),
                message,
                messageStyle
            );
        }

        GUI.color = oldColor;
    }
}
