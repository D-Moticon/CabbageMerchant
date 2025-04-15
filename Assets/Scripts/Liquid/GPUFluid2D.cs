using UnityEngine;
using UnityEngine.Rendering;

public class GPUFluid2D : MonoBehaviour
{
    public Camera fluidCamera;

    [Header("Compute Shader Setup")]
    public ComputeShader fluidCompute;
    public CollisionCamera2D collisionCamera2D; // Reference to get collisionTexture

    [Header("Simulation Parameters")]
    public int maxParticles = 10000;
    public float particleRadius = 0.05f;
    public Vector2 domainMin = new Vector2(-5, -5);
    public Vector2 domainMax = new Vector2(5, 5);
    public float restitution = 0.5f;
    public Vector2 gravity = new Vector2(0, -9.81f);

    
    
    
    [Header("Rendering")]
    public Material particleMaterial;
    public float particleSize = 0.1f;

    // Buffers
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer velocitiesBuffer;

    // Current alive particle count
    private int currentParticleCount = 0;

    // Kernel IDs
    private int updateKernelID;

    // Thread group sizes
    private const int THREADS = 256;

    void Start()
    {
        // Init buffers
        positionsBuffer = new ComputeBuffer(maxParticles, sizeof(float) * 2);
        velocitiesBuffer = new ComputeBuffer(maxParticles, sizeof(float) * 2);

        // Initialize data
        Vector2[] initPositions = new Vector2[maxParticles];
        Vector2[] initVelocities = new Vector2[maxParticles];
        for (int i = 0; i < maxParticles; i++)
        {
            initPositions[i] = new Vector2(0, -9999); // Off-screen to start
            initVelocities[i] = Vector2.zero;
        }
        positionsBuffer.SetData(initPositions);
        velocitiesBuffer.SetData(initVelocities);

        updateKernelID = fluidCompute.FindKernel("UpdateParticles");
    }

    void Update()
    {
        UpdateFluidDomainFromCamera(fluidCamera);

        float dt = Time.deltaTime;

        // Dispatch compute shader
        fluidCompute.SetBuffer(updateKernelID, "_Positions", positionsBuffer);
        fluidCompute.SetBuffer(updateKernelID, "_Velocities", velocitiesBuffer);

        fluidCompute.SetTexture(updateKernelID, "_CollisionMap", collisionCamera2D.collisionTexture);

        fluidCompute.SetFloat("_DeltaTime", dt);
        fluidCompute.SetFloats("_Gravity", gravity.x, gravity.y);
        fluidCompute.SetFloats("_DomainMin", domainMin.x, domainMin.y);
        fluidCompute.SetFloats("_DomainMax", domainMax.x, domainMax.y);
        fluidCompute.SetFloat("_Restitution", restitution);
        fluidCompute.SetFloat("_ParticleRadius", particleRadius);

        // In a more robust setup, you'd pass currentParticleCount to the shader so it doesn't process
        // all maxParticles. But for simplicity, we just do maxParticles for now.
        int threadGroups = Mathf.CeilToInt((float)maxParticles / THREADS);

        fluidCompute.Dispatch(updateKernelID, threadGroups, 1, 1);

        // Draw the particles
        if (particleMaterial != null)
        {
            particleMaterial.SetBuffer("_Positions", positionsBuffer);
            particleMaterial.SetFloat("_ParticleSize", particleSize);
            // Optionally set other properties, like color, etc.

            // Indirect or direct draw call
            // We draw maxParticles, though only "active" ones matter. 
            // For a real scenario, pass in currentParticleCount.
            Graphics.DrawProcedural(particleMaterial, new Bounds(Vector3.zero, Vector3.one * 10000),
                                    MeshTopology.Points, maxParticles);
        }
    }

    void UpdateFluidDomainFromCamera(Camera cam)
    {
        // The camera center in world space
        Vector3 camPos = cam.transform.position;

        // The camera's half-height in world space
        float halfHeight = cam.orthographicSize;

        // The camera�s aspect ratio (width / height)
        float aspect = cam.aspect;
        float halfWidth = halfHeight * aspect;

        // Now the domain is exactly what the camera sees
        domainMin = new Vector2(camPos.x - halfWidth, camPos.y - halfHeight);
        domainMax = new Vector2(camPos.x + halfWidth, camPos.y + halfHeight);
    }


    void OnDestroy()
    {
        if (positionsBuffer != null) positionsBuffer.Release();
        if (velocitiesBuffer != null) velocitiesBuffer.Release();
    }

    /// <summary>
    /// Public method to spawn a particle in the system.
    /// </summary>
    public void SpawnParticle(Vector2 position, Vector2 velocity)
    {
        if (currentParticleCount >= maxParticles) return;

        // We can set the data directly on the GPU buffer, but that requires reading/writing the entire buffer.
        // For demo, let's read them, modify in CPU, then write back:
        Vector2[] posArray = new Vector2[maxParticles];
        Vector2[] velArray = new Vector2[maxParticles];

        positionsBuffer.GetData(posArray);
        velocitiesBuffer.GetData(velArray);

        posArray[currentParticleCount] = position;
        velArray[currentParticleCount] = velocity;

        positionsBuffer.SetData(posArray);
        velocitiesBuffer.SetData(velArray);

        currentParticleCount++;
    }
}
