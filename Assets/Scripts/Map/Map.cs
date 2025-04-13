using UnityEngine;
using System.Collections.Generic;

public class Map : MonoBehaviour
{
    public LineRenderer mapLinePrefab;
    public MapIcon mapIconPrefab;

    // Instead of going downward (layers.Count * -3f),
    // we go upward (layers.Count * positive spacing).
    public float xOffset = 0f;
    public float xStep   = 2f; // distance between icons horizontally
    public float verticalSpacing = 2f;
    public float randomXOffset = 1.5f;
    public float randomYOffset = 0.65f;
    
    // The generated layers in this map
    public List<MapLayer> layers = new List<MapLayer>();

    [System.Serializable]
    public class MapLayer
    {
        public List<MapIcon> mapIcons;
    }

    /// <summary>
    /// Creates a new layer containing MapIcons for each provided MapPoint,
    /// places them with a simple horizontal layout, and connects them to the previous layer.
    /// </summary>
    public void AddMapLayer(List<MapPoint> mapPoints)
    {
        // Create a new layer
        MapLayer newLayer = new MapLayer { mapIcons = new List<MapIcon>() };
        
        float yPos = layers.Count * verticalSpacing; 
        // The first layer (layer 0) is at y=0,
        // next layer (layer 1) is at y=3, etc.

        for (int i = 0; i < mapPoints.Count; i++)
        {
            MapPoint pointData = mapPoints[i];

            // Instantiate a MapIcon
            MapIcon icon = Instantiate(mapIconPrefab, transform);
            icon.spriteRenderer.sprite = pointData.mapIcon;
            icon.sceneName = pointData.sceneName;

            // Place them left to right, but each layer is placed further up
            float randX = Random.Range(-randomXOffset * 0.5f, randomXOffset * 0.5f);
            float randY = Random.Range(-randomYOffset * 0.5f, randomYOffset * 0.5f);
            icon.transform.localPosition = new Vector3(xOffset + i * xStep, yPos, 0f) + new Vector3(randX, randY, 0f);

            newLayer.mapIcons.Add(icon);
        }

        // If there's a previous layer, connect it with lines
        if (layers.Count > 0)
        {
            MapLayer previousLayer = layers[layers.Count - 1];
            ConnectLayers(previousLayer, newLayer);
        }

        // Finally, add the new layer to our layers list
        layers.Add(newLayer);
    }

    /// <summary>
    /// Draws a line between every MapIcon in 'previousLayer' and every MapIcon in 'newLayer'.
    /// Each line is a new LineRenderer instance.
    /// </summary>
    private void ConnectLayers(MapLayer previousLayer, MapLayer newLayer)
    {
        if (!mapLinePrefab)
        {
            Debug.LogWarning("No mapLinePrefab assigned in Map. Cannot draw lines!");
            return;
        }

        foreach (var oldIcon in previousLayer.mapIcons)
        {
            foreach (var newIcon in newLayer.mapIcons)
            {
                LineRenderer lr = Instantiate(mapLinePrefab, transform);
                lr.positionCount = 2;
                lr.SetPosition(0, oldIcon.transform.position);
                lr.SetPosition(1, newIcon.transform.position);
            }
        }
    }
}
