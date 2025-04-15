// GasFluidManager.cs
using UnityEngine;

public class GasFluidManager : MonoBehaviour
{
    [Header("Compute Shader Settings")]
    public ComputeShader fluidSimCompute;
    public int gridWidth = 256;
    public int gridHeight = 256;
    float initialTimeStep = 0.125f;
    public float cellSize = 1.0f;
    public float viscosity = 0.001f;
    public int jacobiIterations = 40;

    public float externalVelocityMult = 1.0f;

    [Header("Buoyancy / Gravity Settings")]
    [Tooltip("How strongly temperature differences produce upward force.")]
    public float buoyancyCoefficient = 1.0f;

    [Tooltip("How strongly density differences produce downward force.")]
    public float weightCoefficient = 1.0f;

    [Tooltip("Ambient temperature (fluid at this temperature feels no buoyancy).")]
    public float ambientTemperature = 0.0f;

    [Tooltip("Ambient density (fluid at this density feels no weight force).")]
    public float ambientDensity = 0.0f;

    [Header("Turbulence Settings")]
    [Tooltip("Strength of the turbulence force added to the fluid.")]
    public float turbulenceAmplitude = 0.1f;
    [Tooltip("Scale of the turbulence noise (higher values mean more �zoomed in� noise).")]
    public float turbulenceScale = 10.0f;
    public float turbulenceTimeMult = 1.0f;

    [Header("Decay")]
    [Tooltip("Multiplier applied to the density each frame (1 = no decay, <1 = decay over time).")]
    public float densityDecay = 0.99f;
    public float temperatureDecay = 0.99f;
    public float colorDecay = 0.99f;

    [Header("Clamping Settings")]
    [Tooltip("Minimum allowable density.")]
    public float minDensity = 0.0f;
    [Tooltip("Maximum allowable density.")]
    public float maxDensity = 1.0f;
    [Tooltip("Minimum allowable temperature.")]
    public float minTemperature = 0.0f;
    [Tooltip("Maximum allowable temperature.")]
    public float maxTemperature = 1.0f;
    public float maxVelocity = 50.0f;
    

    [Header("Time Settings")]
    // Maximum allowed time step for stability.
    public float timeMultiplier = 4f;
    public int substeps = 2;

    [Header("Environment RenderTextures")]
    [Tooltip("RenderTexture for solid objects. Nonzero values (e.g. in the red channel) mark obstacles.")]
    public RenderTexture solidTexture;
    [Tooltip("RenderTexture for moving objects' velocities (use the red/green channels as velocity components).")]
    public RenderTexture objectVelocityTexture;

    [Header("Fluid Interaction Settings")]
    public RenderTexture fluidRT;  // Assign this in the Inspector (an HDR RT such as R16G16B16A16_SFLOAT)
    public int defaultParticleType = 1; // e.g., 1 = water

    

    // Compute buffers
    ComputeBuffer velocityBuffer;
    ComputeBuffer velocityTempBuffer;
    ComputeBuffer densityBuffer;
    ComputeBuffer densityTempBuffer;
    ComputeBuffer pressureBuffer;
    ComputeBuffer pressureTempBuffer;
    ComputeBuffer divergenceBuffer;
    ComputeBuffer colorBuffer;
    ComputeBuffer colorTempBuffer;
    ComputeBuffer temperatureBuffer;
    ComputeBuffer temperatureTempBuffer;


    int totalCells;

    // Kernel IDs
    int kernelAddForce, kernelAdvectVelocity, kernelAdvectDensity;
    int kernelComputeDivergence, kernelJacobiPressure, kernelSubtractGradient;
    int kernelCopyFloat, kernelCopyFloat2, kernelCopyFloat4;
    int kernelApplyEnvironment;
    int kernelAdvectColor;
    int kernelDecayDensity, kernelDecayColor;
    int kernelAdvectTemperature, kernelCopyTemperature, kernelDecayTemperature;
    int kernelBuoyancy;
    int kernelClamp;
    int kernelTurbulence;

    void Start()
    {
        totalCells = gridWidth * gridHeight;

        // Allocate buffers
        velocityBuffer = new ComputeBuffer(totalCells, sizeof(float) * 2);
        velocityTempBuffer = new ComputeBuffer(totalCells, sizeof(float) * 2);
        densityBuffer = new ComputeBuffer(totalCells, sizeof(float));
        densityTempBuffer = new ComputeBuffer(totalCells, sizeof(float));
        pressureBuffer = new ComputeBuffer(totalCells, sizeof(float));
        pressureTempBuffer = new ComputeBuffer(totalCells, sizeof(float));
        divergenceBuffer = new ComputeBuffer(totalCells, sizeof(float));
        colorBuffer = new ComputeBuffer(totalCells, sizeof(float) * 4);
        colorTempBuffer = new ComputeBuffer(totalCells, sizeof(float) * 4);

        // Initialize buffers to zero.
        Vector2[] zeroVel = new Vector2[totalCells];
        for (int i = 0; i < totalCells; i++) zeroVel[i] = Vector2.zero;
        velocityBuffer.SetData(zeroVel);
        velocityTempBuffer.SetData(zeroVel);

        float[] zeros = new float[totalCells];
        densityBuffer.SetData(zeros);
        densityTempBuffer.SetData(zeros);
        pressureBuffer.SetData(zeros);
        pressureTempBuffer.SetData(zeros);
        divergenceBuffer.SetData(zeros);

        temperatureBuffer = new ComputeBuffer(totalCells, sizeof(float));
        temperatureTempBuffer = new ComputeBuffer(totalCells, sizeof(float));

        // Initialize the temperature buffer to a default value (e.g., 0, meaning no extra temperature)
        float[] defaultTemperature = new float[totalCells];
        for (int i = 0; i < totalCells; i++)
        {
            defaultTemperature[i] = 0.0f;  // or some ambient temperature value
        }
        temperatureBuffer.SetData(defaultTemperature);

        // Initialize the color buffer (for example, all cells start as white, or any default color)
        Color defaultColor = Color.clear;
        float[] defaultColorArray = new float[totalCells * 4];
        for (int i = 0; i < totalCells; i++)
        {
            // Store RGBA as floats in sequence.
            defaultColorArray[i * 4 + 0] = defaultColor.r;
            defaultColorArray[i * 4 + 1] = defaultColor.g;
            defaultColorArray[i * 4 + 2] = defaultColor.b;
            defaultColorArray[i * 4 + 3] = defaultColor.a;
        }
        colorBuffer.SetData(defaultColorArray);

        // Get kernel IDs
        kernelAddForce = fluidSimCompute.FindKernel("AddForce");
        kernelAdvectVelocity = fluidSimCompute.FindKernel("AdvectVelocity");
        kernelAdvectDensity = fluidSimCompute.FindKernel("AdvectDensity");
        kernelComputeDivergence = fluidSimCompute.FindKernel("ComputeDivergence");
        kernelJacobiPressure = fluidSimCompute.FindKernel("JacobiPressure");
        kernelSubtractGradient = fluidSimCompute.FindKernel("SubtractGradient");
        kernelCopyFloat = fluidSimCompute.FindKernel("CopyBufferFloat");
        kernelCopyFloat2 = fluidSimCompute.FindKernel("CopyBufferFloat2");
        kernelApplyEnvironment = fluidSimCompute.FindKernel("ApplyEnvironment");
        kernelAdvectColor = fluidSimCompute.FindKernel("AdvectColor");
        kernelCopyFloat4 = fluidSimCompute.FindKernel("CopyBufferFloat4");
        kernelDecayDensity = fluidSimCompute.FindKernel("DecayDensity");
        kernelDecayColor = fluidSimCompute.FindKernel("DecayColor");
        kernelAdvectTemperature = fluidSimCompute.FindKernel("AdvectTemperature");
        kernelCopyTemperature = fluidSimCompute.FindKernel("CopyTemperature");
        kernelDecayTemperature = fluidSimCompute.FindKernel("DecayTemperature");
        kernelBuoyancy = fluidSimCompute.FindKernel("ApplyBuoyancy");
        kernelClamp = fluidSimCompute.FindKernel("ClampProperties");
        kernelTurbulence = fluidSimCompute.FindKernel("ApplyTurbulence");

        // Set common shader parameters
        fluidSimCompute.SetInt("_GridWidth", gridWidth);
        fluidSimCompute.SetInt("_GridHeight", gridHeight);
        fluidSimCompute.SetFloat("_TimeStep", initialTimeStep);
        fluidSimCompute.SetFloat("_CellSize", cellSize);
        fluidSimCompute.SetFloat("_Viscosity", viscosity);
        fluidSimCompute.SetInt("_JacobiIterations", jacobiIterations);
        fluidSimCompute.SetFloat("_DensityDecay", densityDecay);
        fluidSimCompute.SetFloat("_ExternalVelocityMult", externalVelocityMult);
    }

    void Update()
    {
        int groupsX = Mathf.CeilToInt(gridWidth / 8f);
        int groupsY = Mathf.CeilToInt(gridHeight / 8f);

        float dt = Time.deltaTime / substeps * timeMultiplier;

        for (int i = 0; i < substeps; i++)
        {
            // Set the time step for this substep.
            fluidSimCompute.SetFloat("_TimeStep", dt);

            // --- Environment Effects ---
            // If you have provided solid and velocity textures, use them to affect the fluid.
            if (solidTexture != null && objectVelocityTexture != null)
            {
                // Bind the render textures to the compute shader.
                fluidSimCompute.SetTexture(kernelApplyEnvironment, "_SolidTexture", solidTexture);
                fluidSimCompute.SetTexture(kernelApplyEnvironment, "_ObjectVelocityTexture", objectVelocityTexture);
                // Bind the velocity buffer.
                fluidSimCompute.SetBuffer(kernelApplyEnvironment, "_Velocity", velocityBuffer);
                // Dispatch the kernel.
                fluidSimCompute.Dispatch(kernelApplyEnvironment, groupsX, groupsY, 1);
            }

            // Update the turbulence time value (for a moving noise field, you can use Time.time or a scaled version).
            fluidSimCompute.SetFloat("_TurbulenceTime", turbulenceTimeMult * Time.time);
            fluidSimCompute.SetFloat("_TurbulenceAmplitude", turbulenceAmplitude);
            fluidSimCompute.SetFloat("_TurbulenceScale", turbulenceScale);
            fluidSimCompute.SetBuffer(kernelTurbulence, "_Velocity", velocityBuffer);
            fluidSimCompute.Dispatch(kernelTurbulence, groupsX, groupsY, 1);

            // Set the buoyancy parameters in the compute shader.
            fluidSimCompute.SetFloat("_BuoyancyCoefficient", buoyancyCoefficient);
            fluidSimCompute.SetFloat("_WeightCoefficient", weightCoefficient);
            fluidSimCompute.SetFloat("_AmbientTemperature", ambientTemperature);
            fluidSimCompute.SetFloat("_AmbientDensity", ambientDensity);

            // Bind the relevant buffers.
            // _Temperature, _Density, and _Velocity should already be allocated.
            fluidSimCompute.SetBuffer(kernelBuoyancy, "_Temperature", temperatureBuffer);
            fluidSimCompute.SetBuffer(kernelBuoyancy, "_Density", densityBuffer);
            fluidSimCompute.SetBuffer(kernelBuoyancy, "_Velocity", velocityBuffer);

            // Dispatch the ApplyBuoyancy kernel.
            fluidSimCompute.Dispatch(kernelBuoyancy, groupsX, groupsY, 1);

            // --- Advect Velocity ---
            fluidSimCompute.SetBuffer(kernelAdvectVelocity, "_Velocity", velocityBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectVelocity, "_VelocityTemp", velocityTempBuffer);
            fluidSimCompute.Dispatch(kernelAdvectVelocity, groupsX, groupsY, 1);
            // Copy velocityTemp -> velocity
            fluidSimCompute.SetBuffer(kernelCopyFloat2, "_VelocityTemp", velocityTempBuffer);
            fluidSimCompute.SetBuffer(kernelCopyFloat2, "_Velocity", velocityBuffer);
            fluidSimCompute.Dispatch(kernelCopyFloat2, groupsX, groupsY, 1);

            // --- Compute Divergence ---
            fluidSimCompute.SetBuffer(kernelComputeDivergence, "_Velocity", velocityBuffer);
            fluidSimCompute.SetBuffer(kernelComputeDivergence, "_Divergence", divergenceBuffer);
            fluidSimCompute.Dispatch(kernelComputeDivergence, groupsX, groupsY, 1);

            // --- Pressure Solve (Jacobi iterations) ---
            // Clear pressure buffer (for simplicity, set to zero).
            float[] zeros = new float[totalCells];
            pressureBuffer.SetData(zeros);
            for (int j = 0; j < jacobiIterations; j++)
            {
                // Copy pressureBuffer -> pressureTempBuffer via a copy kernel.
                fluidSimCompute.SetBuffer(kernelCopyFloat, "_DensityTemp", pressureBuffer); // reusing CopyBufferFloat kernel for float buffers
                fluidSimCompute.SetBuffer(kernelCopyFloat, "_Density", pressureTempBuffer);
                fluidSimCompute.Dispatch(kernelCopyFloat, groupsX, groupsY, 1);

                fluidSimCompute.SetBuffer(kernelJacobiPressure, "_PressureTemp", pressureTempBuffer);
                fluidSimCompute.SetBuffer(kernelJacobiPressure, "_Pressure", pressureBuffer);
                fluidSimCompute.SetBuffer(kernelJacobiPressure, "_Divergence", divergenceBuffer);
                fluidSimCompute.Dispatch(kernelJacobiPressure, groupsX, groupsY, 1);
            }

            // --- Subtract Gradient ---
            fluidSimCompute.SetBuffer(kernelSubtractGradient, "_Pressure", pressureBuffer);
            fluidSimCompute.SetBuffer(kernelSubtractGradient, "_Velocity", velocityBuffer);
            fluidSimCompute.Dispatch(kernelSubtractGradient, groupsX, groupsY, 1);

            // --- Advect Density ---
            fluidSimCompute.SetBuffer(kernelAdvectDensity, "_Density", densityBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectDensity, "_Velocity", velocityBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectDensity, "_DensityTemp", densityTempBuffer);
            fluidSimCompute.Dispatch(kernelAdvectDensity, groupsX, groupsY, 1);
            // Copy densityTemp -> density
            fluidSimCompute.SetBuffer(kernelCopyFloat, "_DensityTemp", densityTempBuffer);
            fluidSimCompute.SetBuffer(kernelCopyFloat, "_Density", densityBuffer);
            fluidSimCompute.Dispatch(kernelCopyFloat, groupsX, groupsY, 1);

            // --- Advect Color ---
            // Set buffers for AdvectColor kernel.
            fluidSimCompute.SetBuffer(kernelAdvectColor, "_Color", colorBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectColor, "_Velocity", velocityBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectColor, "_ColorTemp", colorTempBuffer);
            fluidSimCompute.Dispatch(kernelAdvectColor, groupsX, groupsY, 1);

            // Copy colorTemp -> color using the CopyBufferFloat4 kernel.
            fluidSimCompute.SetBuffer(kernelCopyFloat4, "_ColorTemp", colorTempBuffer);
            fluidSimCompute.SetBuffer(kernelCopyFloat4, "_Color", colorBuffer);
            fluidSimCompute.Dispatch(kernelCopyFloat4, groupsX, groupsY, 1);

            //Decay density
            fluidSimCompute.SetFloat("_DensityDecay", densityDecay);  // update it each frame if needed
            fluidSimCompute.SetBuffer(kernelDecayDensity, "_Density", densityBuffer);
            fluidSimCompute.Dispatch(kernelDecayDensity, groupsX, groupsY, 1);

            // --- Decay Color ---
            // Update the _ColorDecay parameter.
            fluidSimCompute.SetFloat("_ColorDecay", colorDecay);
            fluidSimCompute.SetBuffer(kernelDecayColor, "_Color", colorBuffer);
            fluidSimCompute.Dispatch(kernelDecayColor, groupsX, groupsY, 1);

            // --- Advect Temperature ---
            fluidSimCompute.SetBuffer(kernelAdvectTemperature, "_Temperature", temperatureBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectTemperature, "_Velocity", velocityBuffer);
            fluidSimCompute.SetBuffer(kernelAdvectTemperature, "_TemperatureTemp", temperatureTempBuffer);
            fluidSimCompute.Dispatch(kernelAdvectTemperature, groupsX, groupsY, 1);

            // --- Copy TemperatureTemp to Temperature ---

            fluidSimCompute.SetBuffer(kernelCopyTemperature, "_TemperatureTemp", temperatureTempBuffer);
            fluidSimCompute.SetBuffer(kernelCopyTemperature, "_Temperature", temperatureBuffer);
            fluidSimCompute.Dispatch(kernelCopyTemperature, groupsX, groupsY, 1);

            // --- Decay Temperature ---
            fluidSimCompute.SetFloat("_TemperatureDecay", temperatureDecay); // temperatureDecay is a public float you set in the manager.
            fluidSimCompute.SetBuffer(kernelDecayTemperature, "_Temperature", temperatureBuffer);
            fluidSimCompute.Dispatch(kernelDecayTemperature, groupsX, groupsY, 1);

            // Set the clamping parameters.
            fluidSimCompute.SetBuffer(kernelClamp, "_Density", densityBuffer);
            fluidSimCompute.SetBuffer(kernelClamp, "_Temperature", temperatureBuffer);
            fluidSimCompute.SetBuffer(kernelClamp, "_Velocity", velocityBuffer);
            fluidSimCompute.SetFloat("_MinDensity", minDensity);
            fluidSimCompute.SetFloat("_MaxDensity", maxDensity);
            fluidSimCompute.SetFloat("_MinTemperature", minTemperature);
            fluidSimCompute.SetFloat("_MaxTemperature", maxTemperature);
            fluidSimCompute.SetFloat("_MaxVelocity", maxVelocity);
            fluidSimCompute.Dispatch(kernelClamp, groupsX, groupsY, 1);

            // (Optionally, update a RenderTexture from the densityBuffer to visualize the simulation.)
        }


    }

    void OnDestroy()
    {
        if (velocityBuffer != null) velocityBuffer.Release();
        if (velocityTempBuffer != null) velocityTempBuffer.Release();
        if (densityBuffer != null) densityBuffer.Release();
        if (densityTempBuffer != null) densityTempBuffer.Release();
        if (pressureBuffer != null) pressureBuffer.Release();
        if (pressureTempBuffer != null) pressureTempBuffer.Release();
        if (divergenceBuffer != null) divergenceBuffer.Release();
        if (colorBuffer != null) colorBuffer.Release();
        if (colorTempBuffer != null) colorTempBuffer.Release();
        if (temperatureBuffer != null) temperatureBuffer.Release();
        if (temperatureTempBuffer != null) temperatureTempBuffer.Release();
    }

    public void SpawnGasAtPosition(Vector2 worldPos, Vector2 forceVelocity, float forceRadius, float densityInjection, Color forceColor, float temperatureInjection)
    {
        Vector2 uv = Camera.main.WorldToViewportPoint(worldPos);
        Vector2 simPos = new Vector2(uv.x * gridWidth, uv.y * gridHeight);

        int groupsX = Mathf.CeilToInt(gridWidth / 8f);
        int groupsY = Mathf.CeilToInt(gridHeight / 8f);

        fluidSimCompute.SetInts("_ForcePosition", Mathf.RoundToInt(simPos.x), Mathf.RoundToInt(simPos.y));
        fluidSimCompute.SetInt("_ForceRadius", (int)forceRadius);
        fluidSimCompute.SetVector("_ForceVector", forceVelocity);
        fluidSimCompute.SetFloat("_DensityInjection", densityInjection);
        fluidSimCompute.SetVector("_ForceColor", forceColor);
        fluidSimCompute.SetFloat("_TemperatureInjection", temperatureInjection);

        // Bind buffers for the AddForce kernel.
        fluidSimCompute.SetBuffer(kernelAddForce, "_Velocity", velocityBuffer);
        fluidSimCompute.SetBuffer(kernelAddForce, "_Density", densityBuffer);
        fluidSimCompute.SetBuffer(kernelAddForce, "_Color", colorBuffer);
        fluidSimCompute.SetBuffer(kernelAddForce, "_Temperature", temperatureBuffer);
        fluidSimCompute.Dispatch(kernelAddForce, groupsX, groupsY, 1);
    }


    public ComputeBuffer GetDensityBuffer()
    {
        return densityBuffer;
    }

    public ComputeBuffer GetColorBuffer()
    {
        return colorBuffer;
    }

    public ComputeBuffer GetTemperatureBuffer()
    {
        return temperatureBuffer;
    }
}
