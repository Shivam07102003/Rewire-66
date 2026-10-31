using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StorageBlackoutController : MonoBehaviour
{
    [Header("Storage Renderers")]
    public SpriteRenderer[] roomRenderers;

    [Header("Robot Headlight")]
    public RobotHeadlightController robotHeadlight;

    [Header("Global Light")]
    public Light2D globalLight;

    [Header("Blackout Timing")]
    public float blinkDuration = 2f;
    public float blinkInterval = 0.2f;

    [Header("Blackout Color")]
    public Color blackoutColor =
        new Color(0.03f, 0.03f, 0.03f, 1f);

    private Color[] originalColors;
    private float originalGlobalIntensity;

    private bool blackoutStarted;
    private bool powerRestored;
    private Coroutine blackoutRoutine;

    private void Awake()
    {
        if (roomRenderers == null)
            roomRenderers = new SpriteRenderer[0];

        originalColors =
            new Color[roomRenderers.Length];

        for (int i = 0; i < roomRenderers.Length; i++)
        {
            if (roomRenderers[i] != null)
                originalColors[i] =
                    roomRenderers[i].color;
        }

        // Automatically find a Global Light 2D.
        if (globalLight == null)
        {
            Light2D[] lights =
                FindObjectsByType<Light2D>();
            foreach (Light2D light in lights)
            {
                if (light.lightType ==
                    Light2D.LightType.Global)
                {
                    globalLight = light;
                    break;
                }
            }
        }

        if (globalLight != null)
        {
            originalGlobalIntensity =
                globalLight.intensity;

            Debug.Log(
                "Global Light found. Original intensity: " +
                originalGlobalIntensity
            );
        }
        else
        {
            Debug.LogError(
                "No Global Light 2D was found."
            );
        }

        if (robotHeadlight != null)
            robotHeadlight.SetHeadlight(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartBlackout(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryStartBlackout(other);
    }

    private void TryStartBlackout(Collider2D other)
    {
        if (blackoutStarted || powerRestored)
            return;

        PlayerInventory inventory =
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
            return;

        blackoutStarted = true;

        if (globalLight != null)
            globalLight.intensity = 0f;

        if (robotHeadlight != null)
            robotHeadlight.SetHeadlight(true);

        blackoutRoutine =
            StartCoroutine(BlackoutSequence());
    }

    private IEnumerator BlackoutSequence()
    {
        float timer = 0f;
        bool dark = false;

        while (timer < blinkDuration)
        {
            dark = !dark;

            for (int i = 0; i < roomRenderers.Length; i++)
            {
                if (roomRenderers[i] == null)
                    continue;

                roomRenderers[i].color =
                    dark
                    ? blackoutColor
                    : originalColors[i];
            }

            yield return new WaitForSeconds(
                blinkInterval
            );

            timer += blinkInterval;
        }

        for (int i = 0; i < roomRenderers.Length; i++)
        {
            if (roomRenderers[i] != null)
                roomRenderers[i].color =
                    blackoutColor;
        }
    }

    public void RestorePower()
    {
        if (powerRestored)
            return;

        powerRestored = true;

        if (blackoutRoutine != null)
            StopCoroutine(blackoutRoutine);

        for (int i = 0; i < roomRenderers.Length; i++)
        {
            if (roomRenderers[i] != null)
                roomRenderers[i].color =
                    originalColors[i];
        }

        if (globalLight != null)
            globalLight.intensity =
                originalGlobalIntensity;

        if (robotHeadlight != null)
            robotHeadlight.SetHeadlight(false);

        Debug.Log("Storage power restored.");
    }
}