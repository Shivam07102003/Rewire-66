using UnityEngine;
using UnityEngine.UI;

public class RadarToggleSwitchVisual : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform handle;
    public Image track;

    public Color onTrackColor = new Color32(32, 216, 232, 255);
    public Color offTrackColor = new Color32(11, 32, 42, 255);
    public Color onHandleColor = Color.white;
    public Color offHandleColor = new Color32(109, 135, 144, 255);

    public float handleOffset = 10f;

    private void Awake()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (toggle != null)
            toggle.onValueChanged.AddListener(SetVisualState);
    }

    private void OnEnable()
    {
        if (toggle != null)
            SetVisualState(toggle.isOn);
    }

    public void SetVisualState(bool isOn)
    {
        if (handle != null)
        {
            Vector2 position = handle.anchoredPosition;
            position.x = isOn ? handleOffset : -handleOffset;
            handle.anchoredPosition = position;
        }

        if (track != null)
            track.color = isOn ? onTrackColor : offTrackColor;

        Image handleImage =
            handle == null ? null : handle.GetComponent<Image>();

        if (handleImage != null)
            handleImage.color = isOn ? onHandleColor : offHandleColor;
    }
}
