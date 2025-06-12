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
    public SpriteRenderer mapFarBGPrefab; //Instantiate one of these for each biome
    [Tooltip("Vertical padding before the first biome segment")]
    public float farBGPaddingStart = 0f;
    [Tooltip("Vertical padding after the last biome segment")]
    public float farBGPaddingEnd = 0f;
    [Tooltip("Extra vertical height added to every biome background")]
    public float farBGExtraHeight = 0f;    [Tooltip("Vertical offset from the layer Y where each farBG segment begins relative to the map icons")]
    public float farBGLayerYOffset = 0f;
    public float mapBG_YMargin = 5f;
    [Header("Sign Settings")]
    [Tooltip("Prefab for placing biome signs")]
    public MapSign mapSignPrefab;
    public float signXOffset = 0f;
    
    // The generated layers in this map
    public List<MapLayer> layers = new List<MapLayer>();

    [System.Serializable]
    public class MapLayer
    {
        public List<MapIcon> mapIcons;
    }

    /// <summary>
    /// Initialize map background and biome segments.
    /// </summary>
    public void InitializeMap(MapBlueprint mapBlueprint)
    {
        // Number of layers from the blueprint.
        int numLayers = layers.Count;
        if (numLayers < 1)
            numLayers = 1;

        // Icon area height
        float iconAreaHeight = (numLayers - 1) * verticalSpacing;

        // Background height including margins
        float bgHeight = iconAreaHeight + 2f * mapBG_YMargin;

        // Resize mapBG (tiled)
        Vector2 bgSize = mapBG.size;
        bgSize.y = bgHeight;
        mapBG.size = bgSize / mapBG.transform.localScale;

        // Position mapBG so bottom edge at y = -mapBG_YMargin
        Vector3 bgPos = mapBG.transform.localPosition;
        bgPos.y = iconAreaHeight * 0.5f;
        mapBG.transform.localPosition = bgPos;

        // Precompute layer Y positions
        float[] layerY = new float[numLayers + 1];
        for (int i = 0; i < numLayers; i++)
            layerY[i] = i * verticalSpacing;
        layerY[numLayers] = iconAreaHeight;

        // Gather biome change indices
        List<int> biomeStarts = new List<int>();
        List<Biome> biomes = new List<Biome>();
        for (int i = 0; i < mapBlueprint.mapLayers.Count; i++)
        {
            Biome b = mapBlueprint.mapLayers[i].newBiome;
            if (b != null)
            {
                biomeStarts.Add(i);
                biomes.Add(b);
            }
        }

                // Instantiate far BG for each biome segment
        // determine sprite base sorting order
        int baseOrder = mapFarBGPrefab.sortingOrder;
        int segments = biomeStarts.Count;
        for (int idx = 0; idx < segments; idx++)
        {
            int start = biomeStarts[idx];
            int end = (idx + 1 < segments) ? biomeStarts[idx + 1] : numLayers;
            float bottom = layerY[start] + farBGLayerYOffset;
            float top = layerY[end];
            // apply padding on first/last biome
            if (idx == 0)
                bottom -= farBGPaddingStart;
            if (idx == segments - 1)
                top += farBGPaddingEnd;
            // extra height equally top & bottom
            bottom -= farBGExtraHeight * 0.5f;
            top    += farBGExtraHeight * 0.5f;
            float height = top - bottom;

            // Create new far BG
            var farBG = Instantiate(mapFarBGPrefab, transform);
            farBG.sprite = biomes[idx].mapFarBG;

            // Use sprite renderer size (tiled) to span width and height
            farBG.drawMode = SpriteDrawMode.Tiled;
            farBG.size = new Vector2(mapBG.size.x, height);
            farBG.transform.localScale = Vector3.one;
            // adjust sorting order: last segment uses base, earlier stepped down
            farBG.sortingOrder = baseOrder - (segments - 1 - idx);

            // Position centered vertically in segment
            Vector3 pos = farBG.transform.localPosition;
            pos.y = bottom + height * 0.5f;
            farBG.transform.localPosition = pos;

            // Instantiate sign at this biome start
            if (mapSignPrefab != null)
            {
                var sign = Instantiate(mapSignPrefab, transform);

                // position the sign using our new offset:
                Vector3 signPos = new Vector3(
                    signXOffset,         // ← your custom horizontal offset
                    layerY[start],       // exactly at the start‐layer Y
                    0f
                );
                sign.transform.localPosition = signPos;
                sign.SetSignTextFromBiome(biomes[idx]);
            }
        }
    }

    /// <summary>
    /// Creates a new layer containing MapIcons for each provided MapPoint,
    /// places them, and connects to previous.
    /// </summary>
    public void AddMapLayer(List<MapPoint> mapPoints)
    {
        MapLayer newLayer = new MapLayer { mapIcons = new List<MapIcon>() };
        float yPos = layers.Count * verticalSpacing;
        float xOff = (mapPoints.Count - 1) * xStep * 0.5f;

        for (int i = 0; i < mapPoints.Count; i++)
        {
            var data = mapPoints[i];
            var icon = Instantiate(mapIconPrefab, transform);
            icon.spriteRenderer.sprite = data.mapIcon;
            icon.mapPoint = data;
            float randX = Random.Range(-randomXOffset * 0.5f, randomXOffset * 0.5f);
            float randY = Random.Range(-randomYOffset * 0.5f, randomYOffset * 0.5f);
            icon.transform.localPosition = new Vector3(xOffset + i * xStep - xOff, yPos, 0f)
                                        + new Vector3(randX, randY, 0f);
            newLayer.mapIcons.Add(icon);
        }

        if (layers.Count > 0)
            ConnectLayers(layers[layers.Count - 1], newLayer);

        layers.Add(newLayer);
    }

    private void ConnectLayers(MapLayer prev, MapLayer next)
    {
        if (!mapLinePrefab)
        {
            Debug.LogWarning("No mapLinePrefab assigned. Cannot draw lines.");
            return;
        }
        foreach (var o in prev.mapIcons)
            foreach (var n in next.mapIcons)
            {
                var lr = Instantiate(mapLinePrefab, transform);
                lr.positionCount = 2;
                lr.SetPosition(0, o.transform.position);
                lr.SetPosition(1, n.transform.position);
            }
    }
}