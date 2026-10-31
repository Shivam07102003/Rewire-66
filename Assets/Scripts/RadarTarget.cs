using UnityEngine;

public class RadarTarget : MonoBehaviour
{
    public enum TargetType
    {
        Component,
        Fuse,
        CircuitBoard,
        CoolantCanister,
        WireSet,
        BatteryPack,
        AccessCard,
        Key,
        Parasite
    }

    [Header("Radar Target")]
    public TargetType targetType =
        TargetType.Component;

    [Header("Color")]
    public bool useCustomColor = false;

    public Color markerColor =
        Color.white;

    public bool showOnRadar = true;

    private void Awake()
    {
        ApplyTargetColor();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!useCustomColor)
            ApplyTargetColor();
    }
#endif

    private void ApplyTargetColor()
    {
        if (useCustomColor)
            return;

        switch (targetType)
        {
            case TargetType.Fuse:
                markerColor = HexColor("#FFD23F");
                break;

            case TargetType.CircuitBoard:
                markerColor = HexColor("#A855F7");
                break;

            case TargetType.CoolantCanister:
                markerColor = HexColor("#22D3EE");
                break;

            case TargetType.WireSet:
                markerColor = HexColor("#fc04cb");
                break;

            case TargetType.BatteryPack:
                markerColor = HexColor("#22C55E");
                break;

            case TargetType.AccessCard:
                markerColor = HexColor("#00ff00");
                break;

            case TargetType.Key:
                markerColor = HexColor("#F59E0B");
                break;

            case TargetType.Parasite:
                markerColor = HexColor("#FF1744");
                break;

            default:
                markerColor = Color.white;
                break;
        }
    }

    private Color HexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.white;
    }
}