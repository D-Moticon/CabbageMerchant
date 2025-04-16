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

    public SpriteRenderer mapBG;
    public float mapBG_YMargin = 5f;
    
    // The generated layers in this map
    public List<MapLayer> layers = new List<MapLayer>();

    [System.Serializable]
    public class MapLayer
    {
        public List<MapIcon> mapIcons;
    }

    public void InitializeMap(MapBlueprint mapBlueprint)
    {
        // Number of layers from the blueprint.
        int numLayers = mapBlueprint.mapLayers.Count;
        if (numLayers < 1)
            numLayers = 1; // Ensure we have at least one layer.
    
        // Icon area: first layer at y=0 and last layer at y = (numLayers - 1)*verticalSpacing.
        float iconAreaHeight = (numLayers - 1) * verticalSpacing;
    
        // The desired background height is the icon area plus a margin on top and bottom.
        float bgHeight = iconAreaHeight + (2 * mapBG_YMargin);
    
        // Adjust the map background sprite size (assuming Draw Mode is Tiled)
        Vector2 bgSize = mapBG.size;
        bgSize.y = bgHeight;
        mapBG.size = bgSize/mapBG.transform.localScale;
    
        // Reposition the background so that its bottom edge is at y = -mapBG_YMargin.
        // With a pivot at center, the bottom edge is at (position.y - bgHeight/2).
        // We want:
        //      position.y - (bgHeight/2) == -mapBG_YMargin
        // Solving, position.y = iconAreaHeight/2  (since bgHeight = iconAreaHeight + 2*mapBG_YMargin).
        Vector3 bgPos = mapBG.transform.localPosition;
        bgPos.y = iconAreaHeight / 2f;
        mapBG.transform.localPosition = bgPos;
    
        Debug.Log($"Map Initialized: Layers = {numLayers}, IconAreaHeight = {iconAreaHeight}, BG Height = {bgHeight}");
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

        float xOff = (mapPoints.Count-1) * xStep * 0.5f;
        
        for (int i = 0; i < mapPoints.Count; i++)
        {
            MapPoint pointData = mapPoints[i];

            // Instantiate a MapIcon
            MapIcon icon = Instantiate(mapIconPrefab, transform);
            icon.spriteRenderer.sprite = pointData.mapIcon;
            icon.mapPoint = pointData;

            // Place them left to right, but each layer is placed further up
            float randX = Random.Range(-randomXOffset * 0.5f, randomXOffset * 0.5f);
            float randY = Random.Range(-randomYOffset * 0.5f, randomYOffset * 0.5f);
            icon.transform.localPosition = new Vector3(xOffset + i * xStep-xOff, yPos, 0f) + new Vector3(randX, randY, 0f);

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
