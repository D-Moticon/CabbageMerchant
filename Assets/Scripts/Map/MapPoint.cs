using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapPoint", menuName = "Scriptable Objects/MapPoint")]
public class MapPoint : ScriptableObject
{
    public string sceneName;
    public string displayName;
    public string description;
    public Sprite mapIcon;
    [SerializeReference]
    public List<MapPointExtra> mapPointExtras = new List<MapPointExtra>();
}
