using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LiquidTypeLibrary", menuName = "Scriptable Objects/LiquidTypeLibrary")]
public class LiquidTypeLibrary : ScriptableObject
{
    public List<LiquidType> liquidTypes;
}
