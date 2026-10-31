using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndScreen : MonoBehaviour
{
    [Header("End Game Audio")]
    public AudioSource audioSource;
    public AudioClip victorySound;
    public AudioClip gameOverSound;

    private static GameEndScreen instance;

    public static bool IsGameEnded
    {
        get;
        private set;
    }

    private static bool victory;
    private static string resultMessage;

    private void Awake()
    {
        instance = this;
        IsGameEnded = false;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void ShowVictory()
    {
        PlayEndSound(true);

        ShowEndScreen(
            true,
            "RE:WIRE 66 COMPLETE"
        );
    }

    public static void ShowDefeat()
    {
        PlayEndSound(false);

        ShowEndScreen(
            false,
            "GAME OVER — ROBOT DISABLED"
        );
    }

    private static void PlayEndSound(bool won)
    {
        if (IsGameEnded)
            return;

        if (instance == null || instance.audioSource == null)
            return;

        AudioClip clip = won
            ? instance.victorySound
            : instance.gameOverSound;

        if (clip != null)
            instance.audioSource.PlayOneShot(clip);
    }

    private static void ShowEndScreen(
        bool won,
        string message
    )
    {
        if (IsGameEnded)
            return;

        IsGameEnded = true;
        victory = won;
        resultMessage = message;

        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (!IsGameEnded)
            return;

        GUI.color =
            new Color(0f, 0f, 0f, 0.85f);

        GUI.Box(
            new Rect(
                0f,
                0f,
                Screen.width,
                Screen.height
            ),
            ""
        );

        GUI.color = Color.white;

        GUIStyle messageStyle =
            new GUIStyle(GUI.skin.label);

        messageStyle.fontSize = 42;
        messageStyle.fontStyle = FontStyle.Bold;
        messageStyle.alignment =
            TextAnchor.MiddleCenter;

        messageStyle.normal.textColor =
            victory
                ? Color.green
                : Color.red;

        GUI.Label(
            new Rect(
                Screen.width / 2f - 400f,
                Screen.height / 2f - 130f,
                800f,
                80f
            ),
            resultMessage,
            messageStyle
        );

        GUIStyle buttonStyle =
            new GUIStyle(GUI.skin.button);

        buttonStyle.fontSize = 24;

        if (GUI.Button(
                new Rect(
                    Screen.width / 2f - 180f,
                    Screen.height / 2f + 10f,
                    160f,
                    60f
                ),
                "RESTART",
                buttonStyle))
        {
            RestartGame();
        }

        if (GUI.Button(
                new Rect(
                    Screen.width / 2f + 20f,
                    Screen.height / 2f + 10f,
                    160f,
                    60f
                ),
                "EXIT",
                buttonStyle))
        {
            ExitGame();
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
