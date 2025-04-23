using UnityEngine;
using System.Collections.Generic;

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
            List<MapPoint> chosenPoints = new List<MapPoint>();
            if (blueprintLayer.forceAll)
            {
                chosenPoints.AddRange(blueprintLayer.possiblePoints);
            }
            else
            {
                var copy = new List<MapPoint>(blueprintLayer.possiblePoints);
                int count = Random.Range(1, 4);
                count = Mathf.Min(count, copy.Count);
                for (int j = 0; j < count; j++)
                {
                    int idx = Random.Range(0, copy.Count);
                    chosenPoints.Add(copy[idx]);
                    copy.RemoveAt(idx);
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
            int value = Mathf.CeilToInt(
                mapBlueprint.metacurrencyBase *
                Mathf.Pow(layerNum, mapBlueprint.metacurrencyPower)
            );

            // spawn at midpoints, skip overlaps via collider check
            foreach (var iconA in prevLayer.mapIcons)
            {
                foreach (var iconB in nextLayer.mapIcons)
                {
                    Vector3 mid = (iconA.transform.position + iconB.transform.position) * 0.5f;
                    // skip if already a pickup here
                    Collider2D[] hits = Physics2D.OverlapCircleAll(mid, 0.05f);
                    bool exists = false;
                    foreach (var hit in hits)
                    {
                        if (hit.GetComponent<MetaCurrencyPickup>() != null)
                        {
                            exists = true;
                            break;
                        }
                    }
                    
                    if (exists) continue;

                    // instantiate pickup
                    GameObject go = Instantiate(metacurrencyPickupPrefab, mid, Quaternion.identity, newMap.transform);
                    var pickup = go.GetComponent<MetaCurrencyPickup>();

                    if (pickup != null)
                    {
                        pickup.SetValue(value);
                    }
                }
            }
        }

        currentMap = newMap;
        return newMap;
    }
}
