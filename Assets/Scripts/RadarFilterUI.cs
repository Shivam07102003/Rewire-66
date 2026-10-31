using UnityEngine;
using UnityEngine.UI;

public class RadarFilterUI : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip uiOpenSound;
    public AudioClip uiCloseSound;

    [Header("Filter UI")]
    public GameObject filterPanel;
    public Button filterButton;

    [Header("Legend UI")]
    public GameObject legendPanel;
    public Button legendButton;

    [Header("Category Toggles")]
    public Toggle parasitesToggle;
    public Toggle repairComponentsToggle;
    public Toggle repairPanelsToggle;
    public Toggle accessItemsToggle;

    public static RadarFilterUI Instance { get; private set; }

    public static bool IsFilterOpen { get; private set; }

    private void Awake()
    {
        Instance = this;
        IsFilterOpen = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (filterPanel != null)
            filterPanel.SetActive(false);

        if (legendPanel != null)
            legendPanel.SetActive(false);

        if (filterButton != null)
            filterButton.onClick.AddListener(ToggleFilterPanel);

        if (legendButton != null)
            legendButton.onClick.AddListener(ToggleLegendPanel);

        AddToggleListener(parasitesToggle);
        AddToggleListener(repairComponentsToggle);
        AddToggleListener(repairPanelsToggle);
        AddToggleListener(accessItemsToggle);
    }

    private void AddToggleListener(Toggle toggle)
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(_ => RefreshRadar());
    }

    public void ToggleFilterPanel()
    {
        if (filterPanel == null)
            return;

        bool opening = !filterPanel.activeSelf;

        if (opening && legendPanel != null)
            legendPanel.SetActive(false);

        filterPanel.SetActive(opening);
        IsFilterOpen = opening;

        PlayUISound(opening);

        if (opening)
            TaskInputBlocker.Block();
        else
            TaskInputBlocker.Unblock();
    }

    public void ToggleLegendPanel()
    {
        if (legendPanel == null)
            return;

        bool opening = !legendPanel.activeSelf;

        if (opening && filterPanel != null && filterPanel.activeSelf)
        {
            filterPanel.SetActive(false);
            IsFilterOpen = false;
            PlayUISound(false);
            TaskInputBlocker.Unblock();
        }

        legendPanel.SetActive(opening);

        PlayUISound(opening);
    }

    private void PlayUISound(bool opening)
    {
        if (audioSource == null)
            return;

        AudioClip clip = opening
            ? uiOpenSound
            : uiCloseSound;

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public static bool IsCategoryVisible(RadarTarget target)
    {
        if (target == null || Instance == null)
            return true;

        RadarTargetCategory category =
            target.GetComponentInParent<RadarTargetCategory>();

        if (category == null)
            return true;

        return Instance.IsEnabled(category.category);
    }

    private bool IsEnabled(RadarCategory category)
    {
        switch (category)
        {
            case RadarCategory.Parasites:
                return parasitesToggle == null || parasitesToggle.isOn;

            case RadarCategory.RepairComponents:
                return repairComponentsToggle == null ||
                       repairComponentsToggle.isOn;

            case RadarCategory.RepairPanels:
                return repairPanelsToggle == null ||
                       repairPanelsToggle.isOn;

            case RadarCategory.AccessItems:
                return accessItemsToggle == null ||
                       accessItemsToggle.isOn;

            default:
                return true;
        }
    }

    private void RefreshRadar()
    {
        // RadarUI_CenteredPlayer reads the toggle state automatically.
    }

    private void OnDestroy()
    {
        if (IsFilterOpen)
            TaskInputBlocker.Unblock();

        IsFilterOpen = false;
    }
}
