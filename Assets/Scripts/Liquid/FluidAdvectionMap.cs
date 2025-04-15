using UnityEngine;

[RequireComponent(typeof(LiquidManager))]
public class FluidAdvectionMap : MonoBehaviour
{
    public ComputeShader advectionCompute;
    public RenderTexture advectionMap;
    public float velocityScale = 1.0f;
    public float densityScale = 1.0f;

    private LiquidManager fluidSim;
    private int kernelIndex;

    void Start()
    {
        fluidSim = GetComponent<LiquidManager>();

        if (advectionMap != null && !advectionMap.enableRandomWrite)
        {
            Debug.LogError("Advection map render texture must have 'Enable Random Write' checked!");
            return;
        }

        kernelIndex = advectionCompute.FindKernel("GenerateAdvectionMap");
    }

    void Update()
    {
        if (advectionMap == null) return;

        // Set compute shader parameters
        advectionCompute.SetBuffer(kernelIndex, "_Positions", fluidSim.positionsBuffer);
        advectionCompute.SetBuffer(kernelIndex, "_Velocities", fluidSim.velocitiesBuffer);
        advectionCompute.SetBuffer(kernelIndex, "_Densities", fluidSim.densitiesBuffer);

        // Set grid data buffers
        advectionCompute.SetBuffer(kernelIndex, "_CellCount", fluidSim.cellCountBuffer);
        advectionCompute.SetBuffer(kernelIndex, "_CellStart", fluidSim.cellStartBuffer);
        advectionCompute.SetBuffer(kernelIndex, "_SortedIndices", fluidSim.sortedIndicesBuffer);

        advectionCompute.SetTexture(kernelIndex, "_AdvectionMap", advectionMap);

        advectionCompute.SetFloats("_DomainMin", fluidSim.domainMin.x, fluidSim.domainMin.y);
        advectionCompute.SetFloats("_DomainMax", fluidSim.domainMax.x, fluidSim.domainMax.y);
        advectionCompute.SetFloat("_KernelRadius", fluidSim.kernelRadius);
        advectionCompute.SetFloat("_CellSize", fluidSim.cellSize);
        advectionCompute.SetInt("_NumCellsX", fluidSim.numCellsX);
        advectionCompute.SetInt("_NumCellsY", fluidSim.numCellsY);
        advectionCompute.SetFloat("_VelocityScale", velocityScale);
        advectionCompute.SetFloat("_DensityScale", densityScale);
        advectionCompute.SetInt("_TextureWidth", advectionMap.width);
        advectionCompute.SetInt("_TextureHeight", advectionMap.height);

        // Calculate thread groups based on texture dimensions
        int threadGroupsX = Mathf.CeilToInt(advectionMap.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(advectionMap.height / 8.0f);
        advectionCompute.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);
    }
}