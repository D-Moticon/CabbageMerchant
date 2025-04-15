using UnityEngine;

[RequireComponent(typeof(LiquidManager))]
public class SPHFluid2D_QuadRender : MonoBehaviour
{
    [Header("Rendering")]
    public Mesh quadMesh;             // A 1x1 quad in local space (corners at +/-0.5)
    public Material particleMaterial;  // Uses "QuadParticleURP" shader
    public float particleSize = 0.2f;
    public bool enableStretching = false;
    public float stretchFactor = 0.1f;
    public float squeezeFactor = 0f;
    public float minimumStretchSpeed = 2f;
    public Camera fluidRenderCam;
    public LiquidTypeLibrary liquidTypeLibrary;

    // We'll grab the fluid references from the same GameObject
    private LiquidManager _fluid;

    void Awake()
    {
        // The SPHFluid2D script is on the same GameObject (due to [RequireComponent])
        _fluid = GetComponent<LiquidManager>();

        for (int i = 0; i < liquidTypeLibrary.liquidTypes.Count; i++)
        {
            string tn = (i + 1).ToString();
            particleMaterial.SetColor($"_Type{tn}Color", liquidTypeLibrary.liquidTypes[i].renderTextureColor);
        }
    }

    void Update()
    {
        if (_fluid.positionsBuffer == null || _fluid.velocitiesBuffer == null) return;

        int count = _fluid.currentParticleCount;
        if (count <= 0) return; // No particles to draw

        // 1) Pass the positions and velocities buffer to the material
        particleMaterial.SetBuffer("_Positions", _fluid.positionsBuffer);
        particleMaterial.SetBuffer("_Velocities", _fluid.velocitiesBuffer);
        particleMaterial.SetInt("_ParticleCount", _fluid.currentParticleCount);
        particleMaterial.SetBuffer("_ParticleCellAndType", _fluid.particleCellAndTypeBuffer);

        // 2) Set rendering properties
        particleMaterial.SetFloat("_ParticleSize", particleSize);
        particleMaterial.SetFloat("_EnableStretching", enableStretching ? 1.0f : 0.0f);
        particleMaterial.SetFloat("_StretchFactor", stretchFactor);
        particleMaterial.SetFloat("_SqueezeFactor", squeezeFactor);
        particleMaterial.SetFloat("_MinimumStretchSpeed", minimumStretchSpeed);

        // 3) Draw the mesh instanced for 'count' instances
        RenderParams rParams = new RenderParams(particleMaterial);
        rParams.camera = fluidRenderCam;
        rParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
        rParams.layer = LayerMask.NameToLayer("Fluid");
        Graphics.RenderMeshPrimitives(rParams, quadMesh, 0, _fluid.maxParticles);
    }
}
