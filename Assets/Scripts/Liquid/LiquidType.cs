using UnityEngine;

[CreateAssetMenu(fileName = "LiquidType", menuName = "Scriptable Objects/LiquidType")]
public class LiquidType : ScriptableObject
{
    public int typeID = 0;
    public Color renderTextureColor = Color.white;
    public float viscosity = 30f;
    public float density = 1200f;
}
