using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip fullHealthSound;

    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDead
    {
        get;
        private set;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;

        currentHealth -= damage;
        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        if (damage > 0f &&
            audioSource != null &&
            damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        Debug.Log(
            "Robot energy: " +
            currentHealth
        );

        if (currentHealth <= 0f)
        {
            IsDead = true;

            Debug.Log("Robot disabled!");

            GameEndScreen.ShowDefeat();
        }
    }

    // RobotSolarRecharge will automatically find
    // and use this method through reflection.
    public void RestoreHealth(float amount)
    {
        if (IsDead)
            return;

        float previousHealth = currentHealth;

        currentHealth += amount;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        if (previousHealth < maxHealth &&
            currentHealth >= maxHealth &&
            audioSource != null &&
            fullHealthSound != null)
        {
            audioSource.PlayOneShot(fullHealthSound);
        }
    }

    private void OnGUI()
    {
        float panelWidth = 270f;
        float panelHeight = 58f;

        Rect panelRect = new Rect(
            24f,
            Screen.height - 82f,
            panelWidth,
            panelHeight
        );

        Color oldColor = GUI.color;

        GUI.color =
            new Color(0.01f, 0.025f, 0.05f, 0.94f);

        GUI.Box(panelRect, "");

        GUI.color =
            new Color(0f, 0.85f, 1f, 0.95f);

        GUI.DrawTexture(
            new Rect(
                panelRect.x,
                panelRect.y,
                panelWidth,
                3f
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        float healthPercent =
            Mathf.Clamp01(
                currentHealth / maxHealth
            );

        Rect barBackground = new Rect(
            panelRect.x + 16f,
            panelRect.y + 34f,
            panelWidth - 32f,
            10f
        );

        GUI.color =
            new Color(0f, 0f, 0f, 0.9f);

        GUI.Box(barBackground, "");

        GUI.color =
            IsDead
                ? Color.red
                : healthPercent <= 0.25f
                    ? new Color(1f, 0.25f, 0.1f)
                    : new Color(0f, 0.85f, 1f);

        GUI.DrawTexture(
            new Rect(
                barBackground.x,
                barBackground.y,
                barBackground.width *
                    healthPercent,
                barBackground.height
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        GUIStyle energyStyle =
            new GUIStyle(GUI.skin.label);

        energyStyle.fontSize = 15;
        energyStyle.fontStyle =
            FontStyle.Bold;
        energyStyle.normal.textColor =
            IsDead ? Color.red : Color.white;

        GUI.Label(
            new Rect(
                panelRect.x + 16f,
                panelRect.y + 8f,
                panelWidth - 32f,
                22f
            ),
            "ENERGY   " +
            Mathf.RoundToInt(currentHealth) +
            " / " +
            Mathf.RoundToInt(maxHealth),
            energyStyle
        );

        GUI.color = oldColor;
    }
}
