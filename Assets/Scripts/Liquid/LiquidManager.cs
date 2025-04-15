using UnityEngine;
using UnityEngine.Serialization;
using Unity.Mathematics;
using UnityEngine.Rendering;

/// <summary>
/// Demonstration SPH fluid manager using a GPU-based uniform grid.
/// Relies on a compute shader that has these kernels:
///   ClearCells, ComputeCellIndices, NaivePrefixSum,
///   WriteSorted, ComputeDensityPressure, ComputeForcesIntegrate.
/// </summary>
public class LiquidManager : MonoBehaviour
{
    [Header("References")]
    public CollisionCamera2D collisionCamera;  // Renders colliders to a texture
    public Material particleMaterial;          // For rendering quads/points
    [FormerlySerializedAs("sphCompute")] public ComputeShader liquidComputeShader;           // The GPU .compute file
    public RenderTexture externalVelocityRenderTexture;
    public RenderTexture advectionRenderTexture;
    public int defaultParticleType = 1; //1=water

    [Header("Rendering")]
    public float particleSize = 1f;
    public bool drawParticles = true;

    [Header("Simulation")]
    [Range(1, 10)] public int subSteps = 2;    // How many substeps per frame

    // Particle capacity
    public int maxParticles = 50000;
    [HideInInspector] public int currentParticleCount = 0;

    // Domain / grid
    public int numCellsX = 100;
    public int numCellsY = 100;
    public float cellSize = 0.4f;  // Usually ~ kernelRadius
    public Vector2 domainMin = new Vector2(-5, -5);
    public Vector2 domainMax = new Vector2(5, 5);

    // SPH parameters
    public float particleRadius = 0.2f;
    public float particleMass = 1f;
    public float restDensity = 1200f;
    public float stiffness = 500f;
    //public float viscosity = 100f;
    public float kernelRadius = 0.3f;
    public float restitution = 0.2f;
    public Vector2 gravity = new Vector2(0, -12f);
    public float externalForceMult = 6f;
    
    [Header("Clamps")]
    // Additional clamps
    public float maxDensity = 5000000f;
    public float minLengthForDensity = 0.0001f;
    public float maxPressure = 1000000f;
    public float maxViscosityForce = 100000f;

    // Internal Buffers
    // Particle data
    [HideInInspector] public ComputeBuffer positionsBuffer;  // public so you can use it in a renderer script if desired
    [HideInInspector] public ComputeBuffer velocitiesBuffer;
    [HideInInspector] public ComputeBuffer densitiesBuffer;
    [HideInInspector] public ComputeBuffer pressuresBuffer;

    // Grid data
    [HideInInspector] public ComputeBuffer cellCountBuffer;   // _CellCount
    [HideInInspector] public ComputeBuffer cellStartBuffer;   // _CellStart
    [HideInInspector] public ComputeBuffer particleCellAndTypeBuffer;// _ParticleCell
    [HideInInspector] public ComputeBuffer sortedIndicesBuffer;// _SortedIndices

    //CPU Mirror of particle cell and type, used in spawning to replace dead particles
    private UInt2[] cpuParticleCellAndType;
    private bool cpuMirrorValid = false;
    private int lastMirrorUpdateFrame = -1;
    
    // Kernel IDs
    int kClearCells;
    int kComputeCellIndices;
    int kNaivePrefixSum;
    int kWriteSorted;
    int kComputeDensityPressure;
    int kComputeForcesIntegrate;

    public struct UInt2
    {
        public uint x;
        public uint y;
        public UInt2(uint x, uint y)
        {
            this.x = x;
            this.y = y;
        }
    }
    
    //New particle Spawning
    Vector2[] newParticles = new Vector2[500];  // Pre-allocate for batching
    Vector2[] newVelocities = new Vector2[500];
    UInt2[] newCellAndTypes = new UInt2[500];
    int spawnIndex = 0;
    
    public struct SpawnParticleData
    {
        public float2 pos;
        public float2 vel;
        public uint type;
    }
    private ComputeBuffer spawnResultBuffer;

    private int oldestParticleIndex = 0;
    private float[] particleSpawnTimes;

    void Start()
    {
        // 1) Allocate main particle buffers
        positionsBuffer = new ComputeBuffer(maxParticles, sizeof(float) * 2);
        velocitiesBuffer = new ComputeBuffer(maxParticles, sizeof(float) * 2);
        densitiesBuffer = new ComputeBuffer(maxParticles, sizeof(float));
        pressuresBuffer = new ComputeBuffer(maxParticles, sizeof(float));

        // 2) Allocate grid buffers
        int numCells = numCellsX * numCellsY;
        cellCountBuffer = new ComputeBuffer(numCells, sizeof(uint));
        cellStartBuffer = new ComputeBuffer(numCells, sizeof(uint));
        particleCellAndTypeBuffer = new ComputeBuffer(maxParticles, sizeof(uint) * 2);
        sortedIndicesBuffer = new ComputeBuffer(maxParticles, sizeof(uint));

        // 3) Get kernel IDs from the compute shader
        kClearCells = liquidComputeShader.FindKernel("ClearCells");
        kComputeCellIndices = liquidComputeShader.FindKernel("ComputeCellIndices");
        kNaivePrefixSum = liquidComputeShader.FindKernel("NaivePrefixSum");
        kWriteSorted = liquidComputeShader.FindKernel("WriteSorted");
        kComputeDensityPressure = liquidComputeShader.FindKernel("ComputeDensityPressure");
        kComputeForcesIntegrate = liquidComputeShader.FindKernel("ComputeForcesIntegrate");

        // 4) (Optional) Set any static parameters that won't change
        // For dynamic ones, we'll set them in Update() each frame.

        particleSpawnTimes = new float[maxParticles];
        cpuParticleCellAndType = new UInt2[maxParticles];
    }

    void Update()
    {
        // Optionally update domain from camera, if you like:
        UpdateFluidDomainFromCamera(collisionCamera.GetComponent<Camera>());

        float dt = Time.deltaTime;
        // clamp dt to avoid spikes
        dt = Mathf.Min(dt, 0.02f);

        // We'll do multiple substeps
        float subDt = dt / (float)subSteps;

        // ----- Set common parameters for ALL kernels -----
        int numCells = numCellsX * numCellsY;

        liquidComputeShader.SetInt("_ParticleCount", currentParticleCount);
        liquidComputeShader.SetInt("_NumCells", numCells);
        liquidComputeShader.SetInt("_NumCellsX", numCellsX);
        liquidComputeShader.SetInt("_NumCellsY", numCellsY);

        liquidComputeShader.SetFloat("_CellSize", cellSize);
        liquidComputeShader.SetFloats("_DomainMin", domainMin.x, domainMin.y);
        liquidComputeShader.SetFloats("_DomainMax", domainMax.x, domainMax.y);

        liquidComputeShader.SetFloat("_DeltaTime", subDt);
        liquidComputeShader.SetFloat("_ParticleRadius", particleRadius);
        liquidComputeShader.SetFloat("_Restitution", restitution);
        liquidComputeShader.SetFloats("_Gravity", gravity.x, gravity.y);

        // SPH constants
        liquidComputeShader.SetFloat("_H", kernelRadius);
        liquidComputeShader.SetFloat("_RestDensity", restDensity);
        liquidComputeShader.SetFloat("_ParticleMass", particleMass);
        liquidComputeShader.SetFloat("_Stiffness", stiffness);
        liquidComputeShader.SetFloat("_ExternalForceMult", externalForceMult);

        liquidComputeShader.SetFloat("_MaxDensity", maxDensity);
        liquidComputeShader.SetFloat("_MinLengthForDensity", minLengthForDensity);
        liquidComputeShader.SetFloat("_MaxPressure", maxPressure);
        liquidComputeShader.SetFloat("_MaxViscosityForce", maxViscosityForce);

        // ----- Bind Buffers to each kernel -----
        // ClearCells
        liquidComputeShader.SetBuffer(kClearCells, "_CellCount", cellCountBuffer);

        // ComputeCellIndices
        liquidComputeShader.SetBuffer(kComputeCellIndices, "_Positions", positionsBuffer);
        liquidComputeShader.SetBuffer(kComputeCellIndices, "_CellCount", cellCountBuffer);
        liquidComputeShader.SetBuffer(kComputeCellIndices, "_ParticleCellAndType", particleCellAndTypeBuffer);

        // NaivePrefixSum
        liquidComputeShader.SetBuffer(kNaivePrefixSum, "_CellCount", cellCountBuffer);
        liquidComputeShader.SetBuffer(kNaivePrefixSum, "_CellStart", cellStartBuffer);

        // WriteSorted
        liquidComputeShader.SetBuffer(kWriteSorted, "_ParticleCellAndType", particleCellAndTypeBuffer);
        liquidComputeShader.SetBuffer(kWriteSorted, "_CellStart", cellStartBuffer);
        liquidComputeShader.SetBuffer(kWriteSorted, "_SortedIndices", sortedIndicesBuffer);

        // ComputeDensityPressure
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_Positions", positionsBuffer);
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_Densities", densitiesBuffer);
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_Pressures", pressuresBuffer);
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_CellCount", cellCountBuffer);
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_CellStart", cellStartBuffer);
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_SortedIndices", sortedIndicesBuffer);
        liquidComputeShader.SetBuffer(kComputeDensityPressure, "_ParticleCellAndType", particleCellAndTypeBuffer);

        // ComputeForcesIntegrate
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_Positions", positionsBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_Velocities", velocitiesBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_Densities", densitiesBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_Pressures", pressuresBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_CellCount", cellCountBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_CellStart", cellStartBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_SortedIndices", sortedIndicesBuffer);
        liquidComputeShader.SetBuffer(kComputeForcesIntegrate, "_ParticleCellAndType", particleCellAndTypeBuffer);

        // Collision texture (for forces kernel)
        if (collisionCamera != null)
        {
            liquidComputeShader.SetTexture(kComputeForcesIntegrate, "_CollisionMap", collisionCamera.collisionTexture);
            liquidComputeShader.SetFloat("_CollisionMapWidth", collisionCamera.collisionTexture.width);
            liquidComputeShader.SetFloat("_CollisionMapHeight", collisionCamera.collisionTexture.height);
        }

        if (externalVelocityRenderTexture != null)
        {
            liquidComputeShader.SetTexture(kComputeForcesIntegrate, "_ForceMap", externalVelocityRenderTexture);
        }


        int groupsForParticles = Mathf.CeilToInt((float)currentParticleCount / 256f);
        int groupsForCells = Mathf.CeilToInt((float)numCells / 256f);

        // ----- Run sub-steps -----
        if (currentParticleCount > 0)
        {
            for (int s = 0; s < subSteps; s++)
            {
                // 1) ClearCells
                liquidComputeShader.Dispatch(kClearCells, groupsForCells, 1, 1);

                // 2) ComputeCellIndices (one thread per particle)
                liquidComputeShader.Dispatch(kComputeCellIndices, groupsForParticles, 1, 1);

                // 3) NaivePrefixSum (single-thread group)
                liquidComputeShader.Dispatch(kNaivePrefixSum, 1, 1, 1);

                // 4) WriteSorted
                liquidComputeShader.Dispatch(kWriteSorted, groupsForParticles, 1, 1);

                // 5) ComputeDensityPressure
                liquidComputeShader.Dispatch(kComputeDensityPressure, groupsForParticles, 1, 1);

                // 6) ComputeForcesIntegrate
                liquidComputeShader.Dispatch(kComputeForcesIntegrate, groupsForParticles, 1, 1);
            }
        }

        // ----- Render Particles -----
        if (drawParticles && particleMaterial != null)
        {
            // Example: using the old "points" approach
            // If you want quads, you'd do a separate script with DrawMeshInstancedProcedural
            particleMaterial.SetBuffer("_Positions", positionsBuffer);
            particleMaterial.SetFloat("_ParticleSize", particleSize);

            // For demonstration, we draw all maxParticles. Could pass currentParticleCount for clarity.
            Graphics.DrawProcedural(
                particleMaterial,
                new Bounds(Vector3.zero, Vector3.one * 9999f),
                MeshTopology.Points,
                maxParticles
            );
        }
        
        //Update the dead particles array async
        UpdateCPUMirrorAsync();
    }
    
    /// <summary>
    /// Example: auto-match the domain to the camera.
    /// Called each frame if you want a dynamic domain.
    /// </summary>
    void UpdateFluidDomainFromCamera(Camera cam)
    {
        if (!cam) return;
        Vector3 camPos = cam.transform.position;
        float halfH = cam.orthographicSize;
        float aspect = cam.aspect;
        float halfW = halfH * aspect;

        domainMin = new Vector2(camPos.x - halfW, camPos.y - halfH);
        domainMax = new Vector2(camPos.x + halfW, camPos.y + halfH);
    }

    void OnDestroy()
    {
        // Release all buffers
        if (positionsBuffer != null) positionsBuffer.Release();
        if (velocitiesBuffer != null) velocitiesBuffer.Release();
        if (densitiesBuffer != null) densitiesBuffer.Release();
        if (pressuresBuffer != null) pressuresBuffer.Release();

        if (cellCountBuffer != null) cellCountBuffer.Release();
        if (cellStartBuffer != null) cellStartBuffer.Release();
        if (particleCellAndTypeBuffer != null) particleCellAndTypeBuffer.Release();
        if (sortedIndicesBuffer != null) sortedIndicesBuffer.Release();
        if (spawnResultBuffer != null) spawnResultBuffer.Release();
    }

    /// <summary>
    /// Spawns a single particle at 'position' with 'velocity'.
    /// Naively reads CPU arrays, writes one element, and sets data back.
    /// For large counts, you'd do a more efficient approach.
    /// </summary>
    public void SpawnParticle(Vector2 position, Vector2 velocity, uint particleType, float spawnRadius = 0.5f, bool replaceOldestIfFull = true)
    {
        if (currentParticleCount < maxParticles)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector2 spawnPos = new Vector2(position.x, position.y) + offset;

            if (spawnIndex > 0)
            {
                FlushParticleBatch();
            }

            newParticles[spawnIndex] = spawnPos;
            newVelocities[spawnIndex] = velocity;
            newCellAndTypes[spawnIndex] = new UInt2(0,particleType);
            particleSpawnTimes[currentParticleCount] = Time.time;
            cpuParticleCellAndType[currentParticleCount] = new UInt2(0, particleType);
            spawnIndex++;

            if (spawnIndex >= newParticles.Length)
            {
                FlushParticleBatch();
            }

            currentParticleCount++;
        }
        
        else if (replaceOldestIfFull)
        {
            int sIndex = 0;

            int deadIndex = GetDeadParticleIndex();
            if (deadIndex >= 0)
            {
                sIndex = deadIndex;
            }

            else
            {
                // Find the oldest particle
                float oldestTime = particleSpawnTimes[0];
                oldestParticleIndex = 0;
                for (int i = 1; i < currentParticleCount; i++)
                {
                    if (particleSpawnTimes[i] < oldestTime)
                    {
                        oldestTime = particleSpawnTimes[i];
                        oldestParticleIndex = i;
                        sIndex = i;
                    }
                }
            }

            // Directly update the oldest particle in the buffer
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector2 spawnPos = new Vector2(position.x, position.y) + offset;

            // Use ComputeBuffer.SetData with a single element and an offset
            Vector2[] singlePosition = new Vector2[] { spawnPos };
            Vector2[] singleVelocity = new Vector2[] { velocity };
            UInt2[] singleCellAndType = new UInt2[] {new UInt2(0,particleType)};

            positionsBuffer.SetData(singlePosition, 0, sIndex, 1);
            velocitiesBuffer.SetData(singleVelocity, 0, sIndex, 1);
            particleCellAndTypeBuffer.SetData(singleCellAndType, 0, sIndex, 1);

            cpuParticleCellAndType[sIndex] = new UInt2(0, particleType);
            
            // Update the spawn time for the replaced particle
            particleSpawnTimes[sIndex] = Time.time;
        }
    }

    public int GetDeadParticleIndex()
    {
        if (!cpuMirrorValid)
        {
            return -1;
        }
        
        // First, try to find a dead particle using our CPU mirror.
        for (int i = 0; i < currentParticleCount; i++)
        {
            if (cpuParticleCellAndType[i].y == 0) // dead particle found
            {
                return i;
            }
        }

        return -1;
    }
    
    
    void FlushParticleBatch()
    {
        if (spawnIndex == 0) return; // Nothing to flush

        // Determine where to place this batch in the GPU buffers.
        // Assume that currentParticleCount already includes the new particles (i.e. it was incremented in SpawnParticle).
        // Then the batch starts at: destIndex = currentParticleCount - spawnIndex.
        int destIndex = currentParticleCount - spawnIndex;
        int copyCount = spawnIndex;

        // Flush the new positions and velocities.
        positionsBuffer.SetData(newParticles, 0, destIndex, copyCount);
        velocitiesBuffer.SetData(newVelocities, 0, destIndex, copyCount);
        particleCellAndTypeBuffer.SetData(newCellAndTypes, 0, destIndex, copyCount);

        spawnIndex = 0; // Reset the batch
    }


    
    
    public void SpawnParticlesFromTexture(Vector2 worldPos, Texture2D spawnTexture, int maxSpawnResults, uint particleType, float spawnScale = 1.0f, float spawnThreshold = 0.5f)
    {
        if (spawnTexture == null || liquidComputeShader == null)
        {
            Debug.LogWarning("Spawn texture or compute shader not set.");
            return;
        }
        
        // Initialize the spawn result buffer if needed.
        maxSpawnResults = spawnTexture.width * spawnTexture.height;
        if (spawnResultBuffer == null || spawnResultBuffer.count != maxSpawnResults)
        {
            if (spawnResultBuffer != null)
                spawnResultBuffer.Release();
            // The stride is sizeof(SpawnParticleData): two floats (8 bytes) + two floats (8 bytes) + one uint (4 bytes) = 20 bytes.
            // For alignment you might round up to 24 bytes.
            spawnResultBuffer = new ComputeBuffer(maxSpawnResults, 20, ComputeBufferType.Append);
        }
        spawnResultBuffer.SetCounterValue(0);
        
        // Set parameters on the compute shader kernel.
        int kernel = liquidComputeShader.FindKernel("SpawnFromTexture");
        
        // Set the input spawn texture.
        liquidComputeShader.SetTexture(kernel, "_SpawnTexture", spawnTexture);
        // Set its size.
        liquidComputeShader.SetInts("_SpawnTextureSize", spawnTexture.width, spawnTexture.height);
        // Set the emitter world position.
        liquidComputeShader.SetVector("_SpawnWorldPos", worldPos);
        // Set the threshold.
        liquidComputeShader.SetFloat("_SpawnThreshold", spawnThreshold);
        // Set the spawn scale.
        liquidComputeShader.SetFloat("_SpawnScale", spawnScale);
        // Bind the output append buffer.
        liquidComputeShader.SetBuffer(kernel, "_SpawnResults", spawnResultBuffer);
        
        // Dispatch the kernel.
        int threadGroupsX = Mathf.CeilToInt(spawnTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(spawnTexture.height / 8.0f);
        liquidComputeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
        
        // Read back the number of spawn results.
        int[] counterData = new int[1];
        ComputeBuffer countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(spawnResultBuffer, countBuffer, 0);
        countBuffer.GetData(counterData);
        int spawnCount = counterData[0];
        countBuffer.Release();
        
        if (spawnCount > 0)
        {
            // Read back the spawn results.
            SpawnParticleData[] results = new SpawnParticleData[spawnCount];
            spawnResultBuffer.GetData(results, 0, 0, spawnCount);
            
            // For each spawn result, add a new particle.
            foreach (var spawn in results)
            {
                // You might call your existing SpawnParticle method.
                SpawnParticle(spawn.pos, spawn.vel, particleType, 0.1f, true);
            }
        }
    }
    
    
    private void UpdateCPUMirrorAsync()
    {
        // Avoid launching multiple readbacks in the same frame.
        if (Time.frameCount == lastMirrorUpdateFrame)
            return;

        if (Time.frameCount < lastMirrorUpdateFrame + 10)
        {
            //update every X frames
            return;
        }
        
        lastMirrorUpdateFrame = Time.frameCount;

        AsyncGPUReadback.Request(particleCellAndTypeBuffer, (AsyncGPUReadbackRequest req) =>
        {
            if (!req.hasError)
            {
                var data = req.GetData<UInt2>();
                // Copy the data into our CPU mirror array.
                data.CopyTo(cpuParticleCellAndType);
                cpuMirrorValid = true;
            }
            else
            {
                cpuMirrorValid = false;
            }
        });
    }
}