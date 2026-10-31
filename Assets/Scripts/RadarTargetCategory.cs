using UnityEngine;

public enum RadarCategory
{
    Parasites,
    RepairComponents,
    RepairPanels,
    AccessItems,
}

public class RadarTargetCategory : MonoBehaviour
{
    public RadarCategory category = RadarCategory.RepairComponents;
}
