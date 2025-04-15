using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CollisionCamera2D : MonoBehaviour
{
    [Header("Settings")]
    public RenderTexture collisionTexture;
    public int textureSize = 512; // Adjust as needed
    public LayerMask collisionLayer;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;                // Must be orthographic for 2D
        _cam.clearFlags = CameraClearFlags.SolidColor;
        _cam.backgroundColor = Color.white;      // Black = empty space
        _cam.cullingMask = collisionLayer;       // Only render collider shapes
        
        CreateRenderTexture();

        _cam.SetReplacementShader(Shader.Find("Unlit/Color"), "RenderType");
    }

    void CreateRenderTexture()
    {
        /*if (collisionTexture != null)
        {
            collisionTexture.Release();
        }

        float screenAspect = (float)Screen.width / Screen.height;
        int height = 512; // or any baseline you like
        int width = Mathf.RoundToInt(height * screenAspect);

        collisionTexture = new RenderTexture(width, height, 24, RenderTextureFormat.RFloat);
        collisionTexture.enableRandomWrite = true;
        collisionTexture.Create();*/

        _cam.targetTexture = collisionTexture;

    }

    void OnValidate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        CreateRenderTexture();
    }
}
