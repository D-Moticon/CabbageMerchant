using System;
using UnityEngine;

[Serializable]
public class MapInsertedLayers
{
    [Tooltip("Which blueprint layer to insert")]
    public MapBlueprint.MapLayer layer;  

    [Tooltip("Insert this layer after every X generated layers (1-based)")]
    public int everyXLayer = 1;
}