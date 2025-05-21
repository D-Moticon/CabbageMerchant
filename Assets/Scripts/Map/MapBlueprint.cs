using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "MapBlueprint", menuName = "Scriptable Objects/MapBlueprint")]
public class MapBlueprint : ScriptableObject
{
    [Header("Difficulty Metrics")]
    public int metacurrencyLayerPeriod = 5;

    [System.Serializable]
    public class MapLayer
    {
        [Tooltip("Legacy list of points. Used only for initial data import.")]
        public List<MapPoint> possiblePoints;
        [Tooltip("Weighted list of points used at runtime.")]
        public List<MapPointInfo> possiblePointInfos;
        public bool forceAll = false;
        public Biome newBiome;
    }

    [System.Serializable]
    public class MapPointInfo
    {
        public MapPoint mapPoint;
        public float weight = 1f;
        [SerializeReference]
        public List<Requirement> requirements = new List<Requirement>();
    }

    [Header("Layers")]
    public List<MapLayer> mapLayers;

    [Button("Copy Points to PointInfos")]
    private void CopyPossiblePointsToInfos()
    {
        if (mapLayers == null)
            return;

        foreach (var layer in mapLayers)
        {
            if (layer.possiblePointInfos == null)
                layer.possiblePointInfos = new List<MapPointInfo>();

            layer.possiblePointInfos.Clear();

            if (layer.possiblePoints != null)
            {
                foreach (var mp in layer.possiblePoints)
                {
                    layer.possiblePointInfos.Add(new MapPointInfo { mapPoint = mp, weight = 1f });
                }
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}