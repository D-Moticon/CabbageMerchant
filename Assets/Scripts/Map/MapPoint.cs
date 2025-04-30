using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapPoint", menuName = "Scriptable Objects/MapPoint")]
public class MapPoint : ScriptableObject
{
    public string sceneName;
    public string displayName;
    [TextArea]public string description;
    public Sprite mapIcon;
    public Biome biome;
    [SerializeReference]
    public List<MapPointExtra> mapPointExtras = new List<MapPointExtra>();
}
