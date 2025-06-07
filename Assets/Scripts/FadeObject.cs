using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

public class FadeObject : MonoBehaviour
{
    public List<Prop> properties;
    public MaterialHandlingMode materialHandlingMode = MaterialHandlingMode.PropertyBlock;
    public bool useCurrentValuesAsStarting = false;
    public bool FadeOnStart;

    private void Start()
    {
        if (FadeOnStart)
        {
            FadeForward();
        }
    }

    public void FadeForward()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(true));
    }

    public void FadeBackward()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(false));
    }

    private IEnumerator Fade(bool fadeIn)
    {
        foreach (var prop in properties)
        {
            StartCoroutine(AnimateProperty(prop, fadeIn));
        }
        yield return null;
    }
    
    private IEnumerator AnimateProperty(Prop prop, bool fadeIn)
    {
        float elapsedTime = 0f;
        Renderer renderer = GetComponent<Renderer>();
        TMP_Text tmpText = GetComponent<TMP_Text>();
        Image image = GetComponent<Image>();
        CanvasGroup cg = GetComponent<CanvasGroup>();
        MaterialPropertyBlock block = null;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (renderer != null && materialHandlingMode == MaterialHandlingMode.PropertyBlock)
        {
            block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
        }

        if (materialHandlingMode == MaterialHandlingMode.MaterialCopy)
        {
            if (image != null)
            {
                Material matCopy = new Material(image.material);
                image.material = matCopy;
            }
        }

        if (materialHandlingMode == MaterialHandlingMode.FontMaterial)
        {
            if (tmpText != null)
            {
            }
        }


        // Determine start and end values based on fadeIn flag
        float startFloat = fadeIn ? prop.materialFloatStartValue : prop.materialFloatEndValue;
        float endFloat = fadeIn ? prop.materialFloatEndValue : prop.materialFloatStartValue;
        Vector4 startVector = fadeIn ? prop.materialVectorStartValue : prop.materialVectorEndValue;
        Vector4 endVector = fadeIn ? prop.materialVectorEndValue : prop.materialVectorStartValue;
        Color startColor = fadeIn ? prop.materialColorStartValue : prop.materialColorEndValue;
        Color endColor = fadeIn ? prop.materialColorEndValue : prop.materialColorStartValue;
        Vector2 startPosition = fadeIn ? prop.startValue : prop.endValue;
        Vector2 endPosition = fadeIn ? prop.endValue : prop.startValue;
        Vector2 startScale = fadeIn ? prop.startValue : prop.endValue;
        Vector2 endScale = fadeIn ? prop.endValue : prop.startValue;
        float startRotation = fadeIn ? prop.rotationStartValue : prop.rotationEndValue;
        float endRotation = fadeIn ? prop.rotationEndValue : prop.rotationStartValue;
        float startAlpha = fadeIn ? prop.alphaStartValue : prop.alphaEndValue;
        float endAlpha = fadeIn ? prop.alphaEndValue : prop.alphaStartValue;
        Color startSRColor = fadeIn ? prop.startSRColor : prop.endSRColor;
        Color endSRColor = fadeIn ? prop.endSRColor : prop.startSRColor;

        if (useCurrentValuesAsStarting)
        {
            if (renderer != null)
            {
                if (prop.propertyType == Prop.PropertyType.MaterialFloatProp)
                    startFloat = renderer.material.GetFloat(prop.materialFloatName);
                if (prop.propertyType == Prop.PropertyType.MaterialColorProp)
                    startColor = renderer.material.GetColor(prop.materialColorName);
                if (prop.propertyType == Prop.PropertyType.MaterialVectorProp)
                    startVector = renderer.material.GetVector(prop.materialVectorName);
            }

            if (image != null)
            {
                if (prop.propertyType == Prop.PropertyType.MaterialFloatProp)
                    startFloat = image.material.GetFloat(prop.materialFloatName);
                if (prop.propertyType == Prop.PropertyType.MaterialColorProp)
                    startColor = image.material.GetColor(prop.materialColorName);
                if (prop.propertyType == Prop.PropertyType.MaterialVectorProp)
                    startVector = image.material.GetVector(prop.materialVectorName);
            }

            if (tmpText != null)
            {
                if (prop.propertyType == Prop.PropertyType.MaterialFloatProp)
                    startFloat = tmpText.fontMaterial.GetFloat(prop.materialFloatName);
                if (prop.propertyType == Prop.PropertyType.MaterialColorProp)
                    startColor = tmpText.fontMaterial.GetColor(prop.materialColorName);
                if (prop.propertyType == Prop.PropertyType.MaterialVectorProp)
                    startVector = tmpText.fontMaterial.GetVector(prop.materialVectorName);
            }

            if (cg != null)
            {
                if (prop.propertyType == Prop.PropertyType.CanvasGroupAlpha)
                {
                    startAlpha = cg.alpha;
                }
            }

            if (sr != null)
            {
                if (prop.propertyType == Prop.PropertyType.SpriteRendererColor)
                {
                    startSRColor = sr.color;
                }
            }

            startScale = transform.localScale;
            startPosition = transform.localPosition;
            startRotation = transform.localRotation.eulerAngles.z;
        }
        
        while (elapsedTime < prop.duration)
        {
            float t = elapsedTime / prop.duration;
            float curveT = prop.curve.Evaluate(t);

            switch (prop.propertyType)
            {
                case Prop.PropertyType.MaterialFloatProp:
                    ApplyMaterialFloat(prop.materialFloatName, Mathf.Lerp(startFloat, endFloat, curveT), renderer, tmpText, image, block);
                    break;
                case Prop.PropertyType.MaterialVectorProp:
                    ApplyMaterialVector(prop.materialVectorName, Vector4.Lerp(startVector, endVector, curveT), renderer, tmpText, image, block);
                    break;
                case Prop.PropertyType.MaterialColorProp:
                    ApplyMaterialColor(prop.materialColorName, Color.Lerp(startColor, endColor, curveT), renderer, tmpText, image, block);
                    break;
                case Prop.PropertyType.Scale:
                    Vector3 scaleValue = Vector3.Lerp(new Vector3(startScale.x, startScale.y, 1), new Vector3(endScale.x, endScale.y, 1), curveT);
                    transform.localScale = scaleValue;
                    break;
                case Prop.PropertyType.Rotation:
                    Quaternion rotationValue = Quaternion.Lerp(Quaternion.Euler(0, 0, startRotation), Quaternion.Euler(0, 0, endRotation), curveT);
                    transform.localRotation = rotationValue;
                    break;
                case Prop.PropertyType.Position:
                    Vector3 positionValue = Vector3.Lerp(new Vector3(startPosition.x, startPosition.y, transform.position.z), new Vector3(endPosition.x, endPosition.y, transform.position.z), curveT);
                    transform.localPosition = positionValue;
                    break;
                case Prop.PropertyType.CanvasGroupAlpha:
                    ApplyCanvasGroupAlpha(Mathf.Lerp(startAlpha, endAlpha, curveT), cg);
                    break;
                case Prop.PropertyType.SpriteRendererColor:
                    ApplySRColor(Color.Lerp(startSRColor, endSRColor, curveT), sr);
                    break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        switch (prop.propertyType)
        {
            case Prop.PropertyType.MaterialFloatProp:
                ApplyMaterialFloat(prop.materialFloatName, endFloat, renderer, tmpText, image, block);
                break;
            case Prop.PropertyType.MaterialVectorProp:
                ApplyMaterialVector(prop.materialVectorName, endVector, renderer, tmpText, image, block);
                break;
            case Prop.PropertyType.MaterialColorProp:
                ApplyMaterialColor(prop.materialColorName, endColor, renderer, tmpText, image, block);
                break;
            case Prop.PropertyType.Scale:
                Vector3 scaleValue = new Vector3(endScale.x, endScale.y, 1);
                transform.localScale = scaleValue;
                break;
            case Prop.PropertyType.Rotation:
                Quaternion rotationValue = Quaternion.Euler(0, 0, endRotation);
                transform.localRotation = rotationValue;
                break;
            case Prop.PropertyType.Position:
                Vector3 positionValue = new Vector3(endPosition.x, endPosition.y, transform.position.z);
                transform.localPosition = positionValue;
                break;
            case Prop.PropertyType.CanvasGroupAlpha:
                ApplyCanvasGroupAlpha(endAlpha, cg);
                break;
            case Prop.PropertyType.SpriteRendererColor:
                ApplySRColor(endSRColor, sr);
                break;
        }
    }

    public void ResetToStartingValues()
    {
        Renderer renderer = GetComponent<Renderer>();
        TMP_Text tmpText = GetComponent<TMP_Text>();
        Image image = GetComponent<Image>();
        CanvasGroup cg = GetComponent<CanvasGroup>();
        MaterialPropertyBlock block = null;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (renderer != null && materialHandlingMode == MaterialHandlingMode.PropertyBlock)
        {
            block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
        }

        if (materialHandlingMode == MaterialHandlingMode.MaterialCopy)
        {
            if (image != null)
            {
                Material matCopy = new Material(image.material);
                image.material = matCopy;
            }
        }

        foreach (var prop in properties)
        {
            switch (prop.propertyType)
            {
                case Prop.PropertyType.MaterialFloatProp:
                    ApplyMaterialFloat(prop.materialFloatName, prop.materialFloatStartValue, renderer, tmpText, image, block);
                    break;
                case Prop.PropertyType.MaterialVectorProp:
                    ApplyMaterialVector(prop.materialVectorName, prop.materialVectorStartValue, renderer, tmpText, image, block);
                    break;
                case Prop.PropertyType.MaterialColorProp:
                    ApplyMaterialColor(prop.materialColorName, prop.materialColorStartValue, renderer, tmpText, image, block);
                    break;
                case Prop.PropertyType.Scale:
                    transform.localScale = new Vector3(prop.startValue.x, prop.startValue.y, 1);
                    break;
                case Prop.PropertyType.Rotation:
                    transform.localRotation = Quaternion.Euler(0, 0, prop.rotationStartValue);
                    break;
                case Prop.PropertyType.Position:
                    transform.localPosition = new Vector3(prop.startValue.x, prop.startValue.y, transform.position.z);
                    break;
                case Prop.PropertyType.CanvasGroupAlpha:
                    ApplyCanvasGroupAlpha(prop.alphaStartValue, cg);
                    break;
                case Prop.PropertyType.SpriteRendererColor:
                    ApplySRColor(prop.startSRColor, sr);
                    break;
            }
        }

        if (block != null && renderer != null)
        {
            renderer.SetPropertyBlock(block);
        }
    }

    // Helper methods for applying material properties
    private void ApplyMaterialFloat(string propName, float value, Renderer renderer, TMP_Text tmpText, Image image, MaterialPropertyBlock block)
    {
        switch (materialHandlingMode)
        {
            case MaterialHandlingMode.MaterialCopy:
                if (renderer != null) renderer.material.SetFloat(propName, value);
                if (image != null) image.material.SetFloat(propName, value);
                break;
            case MaterialHandlingMode.FontMaterial:
                if (tmpText != null) tmpText.fontMaterial.SetFloat(propName, value);
                break;
            case MaterialHandlingMode.PropertyBlock:
                if (block != null)
                {
                    block.SetFloat(propName, value);
                    if (renderer != null) renderer.SetPropertyBlock(block);
                }
                break;
            case MaterialHandlingMode.SharedMaterial:
                if (renderer != null) renderer.material.SetFloat(propName, value);
                if (image != null)
                {
                    image.material.SetFloat(propName, value);
                }
                break;
        }
    }

    private void ApplyMaterialVector(string propName, Vector4 value, Renderer renderer, TMP_Text tmpText, Image image, MaterialPropertyBlock block)
    {
        switch (materialHandlingMode)
        {
            case MaterialHandlingMode.MaterialCopy:
                if (renderer != null) renderer.material.SetVector(propName, value);
                if (image != null) image.material.SetVector(propName, value);
                break;
            case MaterialHandlingMode.FontMaterial:
                if (tmpText != null) tmpText.fontMaterial.SetVector(propName, value);
                break;
            case MaterialHandlingMode.PropertyBlock:
                if (block != null)
                {
                    block.SetVector(propName, value);
                    if (renderer != null) renderer.SetPropertyBlock(block);
                }
                break;
            case MaterialHandlingMode.SharedMaterial:
                if (renderer != null) renderer.material.SetVector(propName, value);
                if (image != null) image.material.SetVector(propName, value);
                break;
        }
    }

    private void ApplyMaterialColor(string propName, Color value, Renderer renderer, TMP_Text tmpText, Image image, MaterialPropertyBlock block)
    {
        switch (materialHandlingMode)
        {
            case MaterialHandlingMode.MaterialCopy:
                if (renderer != null) renderer.material.SetColor(propName, value);
                if (image != null) image.material.SetColor(propName, value);
                break;
            case MaterialHandlingMode.FontMaterial:
                if (tmpText != null)
                {
                    tmpText.fontMaterial.SetColor(propName, value);
                    tmpText.color = value;
                }
                break;
            case MaterialHandlingMode.PropertyBlock:
                if (block != null)
                {
                    block.SetColor(propName, value);
                    if (renderer != null) renderer.SetPropertyBlock(block);
                }
                break;
            case MaterialHandlingMode.SharedMaterial:
                if (renderer != null) renderer.material.SetColor(propName, value);
                if (image != null) image.material.SetColor(propName, value);
                break;
        }
    }

    private void ApplyCanvasGroupAlpha(float value, CanvasGroup cg)
    {
        cg.alpha = value;
    }

    private void ApplySRColor(Color color, SpriteRenderer sr)
    {
        sr.color = color;
    }
}

public enum MaterialHandlingMode
{
    PropertyBlock,
    MaterialCopy,
    FontMaterial,
    SharedMaterial
}

[System.Serializable]
public class Prop
{
    public enum PropertyType
    {
        MaterialFloatProp,
        MaterialVectorProp,
        MaterialColorProp,
        Scale,
        Rotation,
        Position,
        CanvasGroupAlpha,
        SpriteRendererColor
    }

    public PropertyType propertyType;

    [ShowIf("IsMaterialFloatProp")]
    public string materialFloatName;
    [ShowIf("IsMaterialFloatProp")]
    public float materialFloatStartValue;
    [ShowIf("IsMaterialFloatProp")]
    public float materialFloatEndValue;

    [ShowIf("IsMaterialVectorProp")]
    public string materialVectorName;
    [ShowIf("IsMaterialVectorProp")]
    public Vector4 materialVectorStartValue;
    [ShowIf("IsMaterialVectorProp")]
    public Vector4 materialVectorEndValue;

    [ShowIf("IsMaterialColorProp")]
    public string materialColorName;
    [ShowIf("IsMaterialColorProp")]
    [ColorUsage(true, true)]
    public Color materialColorStartValue;
    [ShowIf("IsMaterialColorProp")]
    [ColorUsage(true, true)]
    public Color materialColorEndValue;

    [ShowIf("@this.propertyType == PropertyType.Scale || this.propertyType == PropertyType.Position")]
    public Vector2 startValue;
    [ShowIf("@this.propertyType == PropertyType.Scale || this.propertyType == PropertyType.Position")]
    public Vector2 endValue;

    [ShowIf("IsRotationProp")]
    public float rotationStartValue;
    [ShowIf("IsRotationProp")]
    public float rotationEndValue;

    [ShowIf("IsAlphaProp")]
    public float alphaStartValue;
    [ShowIf("IsAlphaProp")]
    public float alphaEndValue;

    [ShowIf("IsSRColorProp")]
    public Color startSRColor = Color.white;
    [ShowIf("IsSRColorProp")]
    public Color endSRColor = Color.white;

    public float duration;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    // Conditions for showing properties
    private bool IsMaterialFloatProp() => propertyType == PropertyType.MaterialFloatProp;
    private bool IsMaterialVectorProp() => propertyType == PropertyType.MaterialVectorProp;
    private bool IsMaterialColorProp() => propertyType == PropertyType.MaterialColorProp;
    private bool IsRotationProp() => propertyType == PropertyType.Rotation;

    private bool IsAlphaProp() => propertyType == PropertyType.CanvasGroupAlpha;

    private bool IsSRColorProp() => propertyType == PropertyType.SpriteRendererColor;
}

