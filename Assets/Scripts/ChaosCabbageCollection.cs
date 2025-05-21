using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChaosCabbageCollection", menuName = "Scriptable Objects/ChaosCabbageCollection")]
public class ChaosCabbageCollection : ScriptableObject
{
    public List<ChaosCabbageSO> chaosCabbages;
}
