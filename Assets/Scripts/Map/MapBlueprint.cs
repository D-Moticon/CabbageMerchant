using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapBlueprint", menuName = "Scriptable Objects/MapBlueprint")]
public class MapBlueprint : ScriptableObject
{
    [Header("Difficulty Metrics")]
    
    public int metacurrencyLayerPeriod = 5;
    
    [System.Serializable]
    public class MapLayer
    {
        public List<MapPoint> possiblePoints;
        public bool forceAll = false; //all these points will spawn every time
        public Biome newBiome;
    }

    [Header("Layers")]
    public List<MapLayer> mapLayers;
    
    
}
