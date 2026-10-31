using UnityEngine;

[ExecuteAlways]
public class ParasiteWaypointGenerator : MonoBehaviour
{
    [ContextMenu("Create Parasite Waypoints")]
    public void CreateWaypoints()
    {
        CreateWaypoint("WP_EntryRoom", new Vector3(0, 0, 0));
        CreateWaypoint("WP_CentralHub", new Vector3(13, 0, 0));
        CreateWaypoint("WP_Control", new Vector3(28, 0, 0));
        CreateWaypoint("WP_GeneratorA", new Vector3(43.5f, 0, 0));

        CreateWaypoint("WP_Maintenance", new Vector3(22.5f, -13.5f, 0));
        CreateWaypoint("WP_GeneratorB", new Vector3(43.5f, -13.5f, 0));

        CreateWaypoint("WP_Storage", new Vector3(24, 14, 0));
        CreateWaypoint("WP_LoadingBay", new Vector3(45, 14, 0));

        CreateWaypoint(
            "WP_CentralStorageCorridor",
            new Vector3(16.5f, 7, 0)
        );

        CreateWaypoint(
            "WP_CentralMaintenanceCorridor",
            new Vector3(16.5f, -7, 0)
        );

        CreateWaypoint(
            "WP_StorageLoadingCorridor",
            new Vector3(37, 14, 0)
        );

        CreateWaypoint(
            "WP_ControlGeneratorCorridor",
            new Vector3(34, 0, 0)
        );

        CreateWaypoint(
            "WP_MaintenanceGeneratorCorridor",
            new Vector3(34, -13.5f, 0)
        );

        CreateWaypoint(
            "WP_CentralControlCorridor",
            new Vector3(22, 0, 0)
        );

        Debug.Log("Parasite waypoints created.");
    }

    private void CreateWaypoint(
        string waypointName,
        Vector3 worldPosition
    )
    {
        Transform existing = transform.Find(waypointName);

        if (existing == null)
        {
            GameObject waypoint =
                new GameObject(waypointName);

            waypoint.transform.SetParent(transform);
            existing = waypoint.transform;
        }

        existing.position = worldPosition;
        existing.rotation = Quaternion.identity;
        existing.localScale = Vector3.one;
    }
}