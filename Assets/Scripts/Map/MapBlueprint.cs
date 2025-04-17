using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapBlueprint", menuName = "Scriptable Objects/MapBlueprint")]
public class MapBlueprint : ScriptableObject
{
    [Header("Difficulty Metrics")]
    public double firstRoundGoal = 15;
    public float goalBase = 1f; //exponent base
    public float goalPower = 1.2f;
    
    [System.Serializable]
    public class MapLayer
    {
        public List<MapPoint> possiblePoints;
        public bool forceAll = false; //all these points will spawn every time
    }

    [Header("Layers")]
    public List<MapLayer> mapLayers;
    
    
}
