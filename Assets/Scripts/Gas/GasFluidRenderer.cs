// GasFluidRenderer.cs
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GasFluidRenderer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the Fluid Simulation Manager.")]
    public GasFluidManager fluidSimManager;

    [Tooltip("Reference to the Compute Shader (FluidSim.compute).")]
    public ComputeShader fluidSimCompute;

    // RenderTexture that will hold the color visualization.
    public RenderTexture fluidTexture;

    public float maxColorBrightness = 1.5f;
    
    // Kernel ID for copying color to texture.
    private int kernelCopyColor;

    // Grid dimensions (must match the simulation).
    private int gridWidth;
    private int gridHeight;

    // Cached quad material.
    private Material quadMaterial;

    // Thread group size (should match [numthreads(8,8,1)] in the compute shader).
    private const int THREAD_GROUP_SIZE = 8;

    void Start()
    {
        // Get grid dimensions from the simulation manager.
        gridWidth = fluidSimManager.gridWidth;
        gridHeight = fluidSimManager.gridHeight;

        // (Assuming fluidTexture is assigned externally via the Inspector.)
        // Get the kernel ID for copying color to the RenderTexture.
        kernelCopyColor = fluidSimCompute.FindKernel("CopyColorToTexture");

        // Set the grid dimensions in the compute shader.
        fluidSimCompute.SetInt("_GridWidth", gridWidth);
        fluidSimCompute.SetInt("_GridHeight", gridHeight);

        // Get the quad's material and assign the RenderTexture.
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        quadMaterial = renderer.material;
        quadMaterial.mainTexture = fluidTexture;
    }

    void Update()
    {
        // Bind the simulation's color and density buffers to the compute shader.
        fluidSimCompute.SetBuffer(kernelCopyColor, "_Color", fluidSimManager.GetColorBuffer());
        fluidSimCompute.SetBuffer(kernelCopyColor, "_Density", fluidSimManager.GetDensityBuffer());

        // Bind the output texture.
        fluidSimCompute.SetTexture(kernelCopyColor, "_OutputTexture", fluidTexture);
        fluidSimCompute.SetFloat("_MaxColorBrightness",maxColorBrightness);

        // Calculate the number of thread groups.
        int groupsX = Mathf.CeilToInt(gridWidth / (float)THREAD_GROUP_SIZE);
        int groupsY = Mathf.CeilToInt(gridHeight / (float)THREAD_GROUP_SIZE);

        // Dispatch the kernel to copy the color (modulated by density) to the texture.
        fluidSimCompute.Dispatch(kernelCopyColor, groupsX, groupsY, 1);
    }

    void OnDestroy()
    {
        if (fluidTexture != null)
            fluidTexture.Release();
    }
}
