using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Map mapPrefab;

    // Tracks the current map so we can destroy it before generating a new one
    private Map currentMap;

    /// <summary>
    /// Creates a new Map from defaultMapBlueprint,
    /// placing it as a child of the MapGenerator and removing any existing map.
    /// </summary>
    public Map GenerateMap(MapBlueprint mapBlueprint)
    {
        // 1) If we already have a map, remove it
        if (currentMap != null)
        {
            Destroy(currentMap.gameObject);
            currentMap = null;
        }

        // 2) Instantiate a new Map as a child of this MapGenerator
        Map newMap = Instantiate(mapPrefab, transform);
        newMap.transform.localPosition = Vector3.zero; // optional: position it relative to parent

        // 3) For each layer in the MapBlueprint, pick 1-3 random points from possiblePoints
        foreach (var blueprintLayer in mapBlueprint.mapLayers)
        {
            List<MapPoint> chosenPoints = new List<MapPoint>();
            List<MapPoint> copyOfPossible = new List<MapPoint>(blueprintLayer.possiblePoints);

            int numToPick = Random.Range(1, 4); // pick 1-3
            numToPick = Mathf.Min(numToPick, copyOfPossible.Count);

            for (int i = 0; i < numToPick; i++)
            {
                int randIndex = Random.Range(0, copyOfPossible.Count);
                chosenPoints.Add(copyOfPossible[randIndex]);
                copyOfPossible.RemoveAt(randIndex);
            }

            // Add the chosen points to the map
            newMap.AddMapLayer(chosenPoints);
        }

        currentMap = newMap;
        
        return newMap;
    }
}