using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadarUI_CenteredPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RectTransform radarArea;
    public GameObject markerPrefab;
    public RectTransform playerMarker;

    [Header("Radar Settings")]
    public float worldDetectionRadius = 12f;
    public float radarRadius = 90f;
    public float updateInterval = 0.1f;

    private float nextUpdateTime;

    private readonly Dictionary<RadarTarget, RectTransform> markers =
        new Dictionary<RadarTarget, RectTransform>();

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        PutPlayerAtRadarCenter();
    }

    private void Update()
    {
        if (player == null || radarArea == null || markerPrefab == null)
            return;

        PutPlayerAtRadarCenter();

        if (Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + updateInterval;

        UpdateTargets();
        UpdateMarkerPositions();
    }

    private void PutPlayerAtRadarCenter()
    {
        if (playerMarker == null)
            return;

        playerMarker.anchorMin = new Vector2(0.5f, 0.5f);
        playerMarker.anchorMax = new Vector2(0.5f, 0.5f);
        playerMarker.pivot = new Vector2(0.5f, 0.5f);
        playerMarker.anchoredPosition = Vector2.zero;
        playerMarker.gameObject.SetActive(true);
        playerMarker.SetAsLastSibling();
    }

    private void UpdateTargets()
    {
        RadarTarget[] targets = FindObjectsByType<RadarTarget>();
        HashSet<RadarTarget> activeTargets = new HashSet<RadarTarget>();

        foreach (RadarTarget target in targets)
        {
            if (target == null ||
                !target.showOnRadar ||
                !RadarFilterUI.IsCategoryVisible(target))
                continue;

            activeTargets.Add(target);

            if (!markers.ContainsKey(target))
                CreateMarker(target);
        }

        List<RadarTarget> targetsToRemove =
            new List<RadarTarget>();

        foreach (RadarTarget target in markers.Keys)
        {
            if (!activeTargets.Contains(target) || target == null)
                targetsToRemove.Add(target);
        }

        foreach (RadarTarget target in targetsToRemove)
        {
            if (markers.ContainsKey(target))
            {
                if (markers[target] != null)
                    Destroy(markers[target].gameObject);

                markers.Remove(target);
            }
        }
    }

    private void CreateMarker(RadarTarget target)
    {
        GameObject markerObject = Instantiate(markerPrefab, radarArea);
        RectTransform marker = markerObject.GetComponent<RectTransform>();
        Image markerImage = markerObject.GetComponent<Image>();

        if (markerImage != null)
            markerImage.color = target.markerColor;

        markers.Add(target, marker);
    }

    private void UpdateMarkerPositions()
    {
        foreach (KeyValuePair<RadarTarget, RectTransform> pair in markers)
        {
            RadarTarget target = pair.Key;
            RectTransform marker = pair.Value;

            if (target == null || marker == null)
                continue;

            if (!RadarFilterUI.IsCategoryVisible(target))
            {
                marker.gameObject.SetActive(false);
                continue;
            }

            Vector2 worldOffset =
                (Vector2)target.transform.position -
                (Vector2)player.position;

            float distance = worldOffset.magnitude;

            if (distance > worldDetectionRadius)
            {
                marker.gameObject.SetActive(false);
                continue;
            }

            marker.gameObject.SetActive(true);

            Vector2 radarPosition =
                worldOffset / worldDetectionRadius * radarRadius;

            marker.anchoredPosition =
                Vector2.ClampMagnitude(radarPosition, radarRadius);
        }

        PutPlayerAtRadarCenter();
    }
}
