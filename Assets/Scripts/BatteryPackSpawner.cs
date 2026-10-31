using UnityEngine;
using System.Collections.Generic;

public class BatteryPackSpawner : MonoBehaviour
{
    [Header("Battery")]
    public GameObject batteryPackPrefab;

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;

    [Min(1)]
    public int packsToSpawn = 5;

    [Header("Spawn Settings")]
    public bool spawnOnStart = false;

    private bool alreadySpawned;

    private void Start()
    {
        if (spawnOnStart)
            SpawnBatteryPacks();
    }

    public void SpawnBatteryPacks()
    {
        if (alreadySpawned)
            return;

        if (batteryPackPrefab == null)
        {
            Debug.LogError(
                "Battery Pack Spawner has no battery prefab assigned."
            );

            return;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "Battery Pack Spawner has no spawn points."
            );

            return;
        }

        List<Transform> availablePoints =
            new List<Transform>();

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                availablePoints.Add(point);
        }

        int amountToSpawn = Mathf.Min(
            packsToSpawn,
            availablePoints.Count
        );

        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex =
                Random.Range(0, availablePoints.Count);

            Transform selectedPoint =
                availablePoints[randomIndex];

            Instantiate(
                batteryPackPrefab,
                selectedPoint.position,
                selectedPoint.rotation
            );

            availablePoints.RemoveAt(randomIndex);
        }

        alreadySpawned = true;

        Debug.Log(
            "Spawned " +
            amountToSpawn +
            " battery pack(s)."
        );
    }
}