using UnityEngine;
using UnityEngine.UI;

public class FinalFacilityMapUI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RectTransform mapArea;
    public RawImage facilityMapImage;
    public RectTransform playerMarker;
    public Color playerMarkerColor = Color.white;
    public float playerMarkerSize = 50f;

    [Header("Map Image Display")]
    [Tooltip("Width divided by height of the imported facility map PNG.")]
    public float mapAspectRatio = 1.482f;

    [Header("Map Buttons")]
    public Button mapButton;
    public Button closeButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip uiOpenSound;
    public AudioClip uiCloseSound;

    [Header("World Bounds")]
    [Tooltip("World-space bottom-left corner represented by the map image.")]
    public Vector2 worldMinimum = new Vector2(-9f, -5f);

    [Tooltip("World-space top-right corner represented by the map image.")]
    public Vector2 worldMaximum = new Vector2(25f, 5f);

    [Header("Map Controls")]
    public bool mapVisibleAtStart = false;
    public KeyCode toggleMapKey = KeyCode.M;
    public bool allowKeyboardToggle = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (mapButton != null)
            mapButton.onClick.AddListener(OpenMap);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMap);

        if (mapArea != null)
            mapArea.gameObject.SetActive(mapVisibleAtStart);

        ConfigureMapImage();

        if (playerMarker != null)
        {
            playerMarker.anchorMin = new Vector2(0.5f, 0.5f);
            playerMarker.anchorMax = new Vector2(0.5f, 0.5f);
            playerMarker.anchoredPosition = Vector2.zero;
            playerMarker.sizeDelta =
                new Vector2(playerMarkerSize, playerMarkerSize);
            playerMarker.gameObject.SetActive(true);

            Image markerImage =
                playerMarker.GetComponent<Image>();

            if (markerImage != null)
                markerImage.color = playerMarkerColor;
        }
    }

    private void Update()
    {
        if (allowKeyboardToggle &&
            Input.GetKeyDown(toggleMapKey))
        {
            ToggleMap();
        }

        if (player == null || mapArea == null || playerMarker == null)
            return;

        UpdatePlayerMarker();
    }

    public void OpenMap()
    {
        if (mapArea == null || mapArea.gameObject.activeSelf)
            return;

        mapArea.gameObject.SetActive(true);
        PlayUISound(uiOpenSound);
    }

    public void CloseMap()
    {
        if (mapArea == null || !mapArea.gameObject.activeSelf)
            return;

        mapArea.gameObject.SetActive(false);
        PlayUISound(uiCloseSound);
    }

    public void ToggleMap()
    {
        if (mapArea == null)
            return;

        if (mapArea.gameObject.activeSelf)
            CloseMap();
        else
            OpenMap();
    }

    private void PlayUISound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void UpdatePlayerMarker()
    {
        float normalizedX = Mathf.InverseLerp(
            worldMinimum.x,
            worldMaximum.x,
            player.position.x
        );

        float normalizedY = Mathf.InverseLerp(
            worldMinimum.y,
            worldMaximum.y,
            player.position.y
        );

        Vector2 normalizedPosition = new Vector2(
            Mathf.Clamp01(normalizedX),
            Mathf.Clamp01(normalizedY)
        );

        // Keep the marker inside the same aspect-fitted RectTransform as the
        // map image. This prevents stretching and keeps the marker aligned.
        if (facilityMapImage != null &&
            playerMarker.parent != facilityMapImage.transform)
        {
            playerMarker.SetParent(facilityMapImage.transform, false);
        }

        playerMarker.anchorMin = normalizedPosition;
        playerMarker.anchorMax = normalizedPosition;
        playerMarker.anchoredPosition = Vector2.zero;
        playerMarker.sizeDelta =
            new Vector2(playerMarkerSize, playerMarkerSize);
        playerMarker.gameObject.SetActive(true);
        playerMarker.SetAsLastSibling();
    }

    private void ConfigureMapImage()
    {
        if (facilityMapImage == null)
            return;

        AspectRatioFitter fitter =
            facilityMapImage.GetComponent<AspectRatioFitter>();

        if (fitter == null)
            fitter = facilityMapImage.gameObject.AddComponent<AspectRatioFitter>();

        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = mapAspectRatio;
    }
}
