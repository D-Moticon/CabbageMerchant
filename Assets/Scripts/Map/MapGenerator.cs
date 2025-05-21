using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates map layers and inserts metacurrency pickups between every branching path,
/// only at specified layer intervals (metacurrencyLayerPeriod), avoiding overlapping spawns.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Tooltip("Map prefab to instantiate")] 
    public Map mapPrefab;

    [Tooltip("Prefab for metacurrency pickup GameObject")]
    public GameObject metacurrencyPickupPrefab;

    // Tracks the current map so we can destroy it before generating a new one
    private Map currentMap;

    public delegate void MapEvent(Map m, MapBlueprint mbp);
    public static event MapEvent MapGeneratedEvent;

    /// <summary>
    /// Creates a new Map from mapBlueprint as a child of this generator,
    /// clearing any existing map. Spawns pickups between every icon connection
    /// for layers matching metacurrencyLayerPeriod.
    /// </summary>
    public Map GenerateMap(MapBlueprint mapBlueprint)
    {
        // Destroy previous map
        if (currentMap != null)
        {
            Destroy(currentMap.gameObject);
            currentMap = null;
        }

        // Instantiate new map
        Map newMap = Instantiate(mapPrefab, transform);
        newMap.transform.localPosition = Vector3.zero;

        // Build each layer
        foreach (var blueprintLayer in mapBlueprint.mapLayers)
        {
            // Filter infos by requirements
            var validInfos = blueprintLayer.possiblePointInfos?
                .Where(info => info.mapPoint != null
                    && (info.requirements == null || info.requirements.All(req => req.IsRequirementMet())))
                .ToList() ?? new List<MapBlueprint.MapPointInfo>();

            var chosenPoints = new List<MapPoint>();
            if (blueprintLayer.forceAll)
            {
                // Respect requirements: only include validInfos
                chosenPoints.AddRange(validInfos.Select(info => info.mapPoint));
            }
            else
            {
                // Weighted random selection without replacement
                int count = Random.Range(1, 4);
                count = Mathf.Min(count, validInfos.Count);
                var infosCopy = new List<MapBlueprint.MapPointInfo>(validInfos);
                for (int i = 0; i < count; i++)
                {
                    float totalWeight = infosCopy.Sum(info => info.weight);
                    float r = Random.Range(0f, totalWeight);
                    float accum = 0f;
                    MapBlueprint.MapPointInfo selected = null;

                    foreach (var info in infosCopy)
                    {
                        accum += info.weight;
                        if (r <= accum)
                        {
                            selected = info;
                            break;
                        }
                    }

                    if (selected == null)
                        selected = infosCopy.Last();

                    chosenPoints.Add(selected.mapPoint);
                    infosCopy.Remove(selected);
                }
            }

            newMap.AddMapLayer(chosenPoints);
        }

        // Spawn pickups for each connection between layers at given period
        int period = mapBlueprint.metacurrencyLayerPeriod;
        int totalLayers = newMap.layers.Count;
        for (int i = 0; i < totalLayers - 1; i++)
        {
            int layerNum = i + 1; // 1-based
            if (period <= 0 || (layerNum % period) != 0)
                continue;

            var prevLayer = newMap.layers[i];
            var nextLayer = newMap.layers[i + 1];

            // calculate pickup value
            int value = Singleton.Instance.playerStats.currentDifficulty.GetMetaCurrencyForLayer(layerNum);

            // spawn at midpoints, skip overlaps via collider check
            foreach (var iconA in prevLayer.mapIcons)
            {
                foreach (var iconB in nextLayer.mapIcons)
                {
                    Vector3 mid = (iconA.transform.position + iconB.transform.position) * 0.5f;
                    Collider2D[] hits = Physics2D.OverlapCircleAll(mid, 0.05f);
                    if (hits.Any(hit => hit.GetComponent<MetaCurrencyPickup>() != null))
                        continue;

                    GameObject go = Instantiate(metacurrencyPickupPrefab, mid, Quaternion.identity, newMap.transform);
                    var pickup = go.GetComponent<MetaCurrencyPickup>();
                    if (pickup != null)
                        pickup.SetValue(value);
                }
            }
        }

        currentMap = newMap;
        MapGeneratedEvent?.Invoke(currentMap, mapBlueprint);

        return newMap;
    }
}
