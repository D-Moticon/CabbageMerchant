using System;
using UnityEngine;
using Unity.Mathematics; // For int2
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Rendering;

// This struct must exactly match the layout of InteractionResult in your compute shader.
public struct InteractionResult
{
    public Vector2 pos;   // The pixel's position.
    public int2 types;    // (center pixel type, majority surrounding type)
}

[System.Serializable]
public class InteractionEvents
{
    public LiquidType liquidType1;
    public LiquidType liquidType2;
    public bool destroyFluid1 = true;
    public bool destroyFluid2 = true;
    public float destroyRadius = 0.5f;
    public bool generateGas = true;
    [ShowIf("generateGas", true)]
    public float gasGenerationRadius = 0.2f;
    [ShowIf("generateGas", true)]
    public float gasGenerationDensity = 0.7f;
    [ShowIf("generateGas", true)]
    public float gasGenerationTemperature = 3f;
    [ColorUsage(true, true), ShowIf("generateGas", true)]
    public Color gasColor = Color.white;
    
}

public class FluidInteractionManager : MonoBehaviour
{
    [Header("References")]
    // Your fluid simulation’s RenderTexture.
    public RenderTexture fluidRenderTexture;
    // The compute shader (assign the FluidInteraction.compute asset here).
    public ComputeShader fluidInteractionComputeShader;
    public ComputeShader fluidInteractionDestroyShader;
    public LiquidTypeLibrary liquidTypeLibrary;
    
    [Header("Fluid Colors")]
    // How similar two colors must be to be considered equal.
    public float colorThreshold = 0.1f;

    [Header("Interactions")]
    public List<InteractionEvents> interactionEvents;

    [Header("Analysis Settings")]
    [Range(0.1f, 1f)]
    public float resolutionScale = 0.5f;
    public int maxInteractionsPerFrame = 50;
    
    [Header("World Mapping")]
    // Used to convert pixel coordinates to world positions.
    public float worldScale = 1f;
    public Vector2 worldOffset;

    // Compute buffer that will store the output interaction results.
    private ComputeBuffer resultBuffer;
    ComputeBuffer countBuffer;
    // Maximum number of results we allow. (Here we simply allow up to width*height.)
    private int maxResultCount;
    LiquidManager sphFluid;
    
    private int kInteraction;
    int kDestroy;
    
    //Optimizations
    private const int FRAME_DELAY = 2; // Number of frames to delay reading results
    private Queue<AsyncGPUReadbackRequest> pendingReadbacks;
    private Queue<Vector2> interactionPositions;
    private int currentFrame;

    private void Awake()
    {
        for (int i = 0; i < liquidTypeLibrary.liquidTypes.Count; i++)
        {
            string tn = (i + 1).ToString();
            fluidInteractionComputeShader.SetVector($"_Type{tn}Color", liquidTypeLibrary.liquidTypes[i].renderTextureColor);
        }
    }

    void Start()
    {
        sphFluid = GameSingleton.Instance.liquidSim;

        if (fluidRenderTexture == null || fluidInteractionComputeShader == null)
        {
            Debug.LogError("Please assign both the fluid RenderTexture and the compute shader.");
            enabled = false;
            return;
        }

        // Find the compute shader kernels.
        kInteraction = fluidInteractionComputeShader.FindKernel("CSMain");
        kDestroy = fluidInteractionDestroyShader.FindKernel("DestroyParticlesInCell");

        // Set the maximum count to the number of pixels in the texture.
        maxResultCount = fluidRenderTexture.width * fluidRenderTexture.height;
        // IMPORTANT: The stride now must match the size of our InteractionResult struct.
        // InteractionResult contains a Vector2 (2 floats) and an int2 (2 ints). 
        // In C#, sizeof(float)=4 and sizeof(int)=4. So total size = (2*4) + (2*4) = 16 bytes.
        resultBuffer = new ComputeBuffer(maxResultCount, 16, ComputeBufferType.Append);
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        
        pendingReadbacks = new Queue<AsyncGPUReadbackRequest>();
        interactionPositions = new Queue<Vector2>();
        currentFrame = 0;
    }

    void OnDestroy()
    {
        if (resultBuffer != null) resultBuffer.Release();
        if (countBuffer != null) countBuffer.Release();
        pendingReadbacks.Clear();
        interactionPositions.Clear();
    }

    void Update()
    {
        ProcessPendingReadbacks();
        UpdateInteractionShader();
        UpdateDestroyShader();
        
    }

    void UpdateInteractionShader()
    {

        // Compute scaled dimensions.
        int scaledWidth = Mathf.CeilToInt(fluidRenderTexture.width * resolutionScale);
        int scaledHeight = Mathf.CeilToInt(fluidRenderTexture.height * resolutionScale);

        // Create a temporary RenderTexture with the scaled resolution.
        RenderTexture scaledRT =
            RenderTexture.GetTemporary(scaledWidth, scaledHeight, 0, fluidRenderTexture.format);
        scaledRT.filterMode = FilterMode.Bilinear; // Use bilinear filtering if desired.

        // Downsample fluidRenderTexture into scaledRT.
        Graphics.Blit(fluidRenderTexture, scaledRT);
        
        // Reset the counter on the append buffer.
        resultBuffer.SetCounterValue(0);

        // Set shader parameters.
        fluidInteractionComputeShader.SetTexture(kInteraction, "_FluidTexture", scaledRT);
        fluidInteractionComputeShader.SetBuffer(kInteraction, "_Results", resultBuffer);
        fluidInteractionComputeShader.SetInt("_Width", scaledWidth);
        fluidInteractionComputeShader.SetInt("_Height", scaledHeight);
        fluidInteractionComputeShader.SetFloat("_ColorThreshold", colorThreshold);
        // Convert Unity Colors to Vector4 (RGBA) for the shader.
        for (int i = 0; i < liquidTypeLibrary.liquidTypes.Count; i++)
        {
            string tn = (i + 1).ToString();
            fluidInteractionComputeShader.SetVector($"_Type{tn}Color", liquidTypeLibrary.liquidTypes[i].renderTextureColor);
        }

        // Calculate how many thread groups to dispatch.
        int threadGroupX = Mathf.CeilToInt(scaledWidth / 8.0f);
        int threadGroupY = Mathf.CeilToInt(scaledHeight / 8.0f);
        fluidInteractionComputeShader.Dispatch(kInteraction, threadGroupX, threadGroupY, 1);

        ComputeBuffer.CopyCount(resultBuffer, countBuffer, 0);
        var readbackRequest = AsyncGPUReadback.Request(countBuffer);
        pendingReadbacks.Enqueue(readbackRequest);
        
        RenderTexture.ReleaseTemporary(scaledRT);
        
        /*// Read back the number of results appended.
        int[] counterData = { 0 };
        ComputeBuffer.CopyCount(resultBuffer, countBuffer, 0);
        countBuffer.GetData(counterData);   //<-----This line of code is killing performance
        int resultCount = counterData[0];

        // If any results were found, read them back.
        if (resultCount > 0)
        {
            InteractionResult[] results = new InteractionResult[resultCount];
            resultBuffer.GetData(results, 0, 0, resultCount);

            int resultsToUse = Mathf.Min(maxResultCount, results.Length);

            for (int i = 0; i < resultsToUse; i++)
            {
                // result.pos is the pixel coordinate
                // result.types.x is the center pixel's type
                // result.types.y is the majority surrounding type
                Vector2 worldPos = PixelToWorldPosition(results[i].pos*(1f / resolutionScale));
                ProcessInteraction(worldPos, results[i].types.x, results[i].types.y);
            }
        }*/
    }

    // Converts a pixel coordinate to a world position.
    Vector2 PixelToWorldPosition(Vector2 pixelPos)
    {
        // pixelPos is in scaled coordinates; scale it back to full resolution.
        Vector2 fullResPos = pixelPos; // Already scaled in the UpdateInteractionShader call.
    
        // Calculate the scale factors between the full-resolution render texture and the actual screen.
        float scaleX = Screen.width / (float)fluidRenderTexture.width;
        float scaleY = Screen.height / (float)fluidRenderTexture.height;
    
        Vector2 screenPos = new Vector2(fullResPos.x * scaleX, fullResPos.y * scaleY);
        float distanceFromCamera = -Camera.main.transform.position.z;
        Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, distanceFromCamera);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPoint);
        return new Vector2(worldPos.x, worldPos.y);
    }

    void UpdateDestroyShader()
    {
        // Set the simulation grid buffers from the SPH sim.
        fluidInteractionDestroyShader.SetBuffer(kDestroy, "_CellCount", sphFluid.cellCountBuffer);
        fluidInteractionDestroyShader.SetBuffer(kDestroy, "_CellStart", sphFluid.cellStartBuffer);
        fluidInteractionDestroyShader.SetBuffer(kDestroy, "_SortedIndices", sphFluid.sortedIndicesBuffer);

        // Set the particle data buffers.
        fluidInteractionDestroyShader.SetBuffer(kDestroy, "_Positions", sphFluid.positionsBuffer);
        fluidInteractionDestroyShader.SetBuffer(kDestroy, "_ParticleCellAndType", sphFluid.particleCellAndTypeBuffer);

        // Set grid/domain parameters.
        fluidInteractionDestroyShader.SetVector("_DomainMin", sphFluid.domainMin);
        fluidInteractionDestroyShader.SetFloat("_CellSize", sphFluid.cellSize);
        fluidInteractionDestroyShader.SetInt("_NumCellsX", sphFluid.numCellsX);
        fluidInteractionDestroyShader.SetInt("_NumCellsY", sphFluid.numCellsY);
        // (Additional parameters can be set as needed.)
    }

    // Called when an interaction is detected at a given world position.
    void ProcessInteraction(Vector2 worldPos, int centerType, int majorityType)
    {
        for (int i = 0; i < interactionEvents.Count; i++)
        {
            InteractionEvents ie = interactionEvents[i];
            
            if ((ie.liquidType1.typeID == centerType && ie.liquidType2.typeID == majorityType)
                || (ie.liquidType2.typeID == centerType && ie.liquidType1.typeID == majorityType))
            {
                if (ie.generateGas)
                {
                    GameSingleton.Instance.gasSim.SpawnGasAtPosition(worldPos,
                        Vector2.zero,
                        ie.gasGenerationRadius,
                        ie.gasGenerationDensity,
                        ie.gasColor,
                        ie.gasGenerationTemperature);
                }

                // Use a list to collect the types we want to destroy
                List<int> destroyTypesList = new List<int>();
                
                // Add types to destroy sequentially, regardless of if they're fluid1 or fluid2
                if (ie.destroyFluid1)
                {
                    destroyTypesList.Add(ie.liquidType1.typeID);
                }
                if (ie.destroyFluid2)
                {
                    destroyTypesList.Add(ie.liquidType2.typeID);
                }

                // Convert to array - important: now both types will be at the start of the array
                int[] destroyTypes = new int[4] { 0, 0, 0, 0 };
                for (int t = 0; t < destroyTypesList.Count; t++)
                {
                    destroyTypes[t] = destroyTypesList[t];
                }

                fluidInteractionDestroyShader.SetVector("_DestroyPos", worldPos);
                fluidInteractionDestroyShader.SetFloat("_DestroyRadius", ie.destroyRadius);
                fluidInteractionDestroyShader.SetInt("_DestroyType1", destroyTypes[0]);
                fluidInteractionDestroyShader.SetInt("_DestroyType2", destroyTypes[1]);
                fluidInteractionDestroyShader.SetInt("_DestroyTypesCount", destroyTypesList.Count);

                fluidInteractionDestroyShader.Dispatch(kDestroy, 1, 1, 1);
                break;
            }
        }
    }
    
    void ProcessPendingReadbacks()
    {
        while (pendingReadbacks.Count > 0 && pendingReadbacks.Peek().done)
        {
            var request = pendingReadbacks.Dequeue();
            
            if (!request.hasError)
            {
                var counterData = request.GetData<int>();
                int resultCount = counterData[0];

                if (resultCount > 0)
                {
                    // Request the actual results asynchronously
                    AsyncGPUReadback.Request(resultBuffer, resultCount * 16, 0, 
                        request => ProcessInteractionResults(request, resultCount));
                }
            }
        }
    }
    
    void ProcessInteractionResults(AsyncGPUReadbackRequest request, int resultCount)
    {
        if (!request.hasError)
        {
            var results = request.GetData<InteractionResult>();
            int resultsToUse = Mathf.Min(maxInteractionsPerFrame, resultCount);

            for (int i = 0; i < resultsToUse; i++)
            {
                Vector2 worldPos = PixelToWorldPosition(results[i].pos * (1f / resolutionScale));
                ProcessInteraction(worldPos, results[i].types.x, results[i].types.y);
            }
        }
    }
}
