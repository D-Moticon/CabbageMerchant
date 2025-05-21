using System;
using UnityEngine;

[Serializable]
public class MaterialPropertyOverride
{
    public enum PropertyType { Float, Color, Vector, Texture }

    public string propertyName;
    public PropertyType propertyType;

    // one of these will be used, according to propertyType:
    public float  floatValue;
    public Color  colorValue;
    public Vector4 vectorValue;
    public Texture textureValue;

    /// <summary>
    /// Applies this override into the given block.
    /// </summary>
    public void ApplyTo(MaterialPropertyBlock block)
    {
        switch (propertyType)
        {
            case PropertyType.Float:
                block.SetFloat(propertyName, floatValue);
                break;
            case PropertyType.Color:
                block.SetColor(propertyName, colorValue);
                break;
            case PropertyType.Vector:
                block.SetVector(propertyName, vectorValue);
                break;
            case PropertyType.Texture:
                block.SetTexture(propertyName, textureValue);
                break;
        }
    }
}