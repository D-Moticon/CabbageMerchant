using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PressureCamera2D : MonoBehaviour
{
    public RenderTexture pressureTexture;
    public int textureSize = 512;
    public LayerMask pressureLayer;
    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.clearFlags = CameraClearFlags.SolidColor;
        _cam.backgroundColor = Color.black; // Black = no pressure
        _cam.cullingMask = pressureLayer;
        CreateRenderTexture();
    }

    void CreateRenderTexture()
    {
        if (pressureTexture != null)
        {
            pressureTexture.Release();
        }

        float screenAspect = (float)Screen.width / Screen.height;
        int height = textureSize;
        int width = Mathf.RoundToInt(height * screenAspect);

        pressureTexture = new RenderTexture(width, height, 24, RenderTextureFormat.RFloat);
        pressureTexture.enableRandomWrite = true;
        pressureTexture.Create();

        _cam.targetTexture = pressureTexture;
    }

    void OnValidate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        CreateRenderTexture();
    }
}
