using UnityEngine;
using UnityEngine.Rendering; // Needed for AsyncGPUReadback

[RequireComponent(typeof(Rigidbody2D))]
public class AdvectedObject : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The RenderTexture containing the fluid simulation data")]
    private RenderTexture advectionMap;

    [Header("Force Settings")]
    [Tooltip("Multiplier for the force applied from the fluid simulation (R and G channels)")]
    public float forceMultiplier = 10f;
    [Tooltip("Multiplier for buoyancy force computed from the density (B channel) of the advection map")]
    public float buoyancyMultiplier = 5f;
    [Tooltip("Multiplier for drag force based on the object's velocity and the fluid density")]
    public float dragMultiplier = 1.0f;
    [Tooltip("How frequently to sample the advection map (in seconds)")]
    public float sampleRate = 0.02f;
    [Tooltip("If true, the Y coordinate is flipped when sampling (adjust if the RenderTexture is upside down)")]
    public bool flipY = false;

    // Private references
    private Rigidbody2D rb;
    private float timeSinceLastSample = 0f;
    private bool isAwaitingReadback = false;

    // Reference to your fluid manager from which we'll grab the simulation domain bounds.
    private LiquidManager fluidManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Get the fluid manager from your singleton instance.
        fluidManager = GameSingleton.Instance.liquidSim;
        if (fluidManager == null)
        {
            Debug.LogError("Fluid Manager is not available via Singleton.Instance.fluidSim!");
        }
        
        advectionMap = fluidManager.advectionRenderTexture;
    }

    private void FixedUpdate()
    {
        timeSinceLastSample += Time.fixedDeltaTime;

        // Only sample if enough time has elapsed and if no previous readback is still in progress.
        if (timeSinceLastSample >= sampleRate && !isAwaitingReadback)
        {
            SampleFluidForce();
            timeSinceLastSample = 0f;
        }
    }

    /// <summary>
    /// Initiates an asynchronous GPU readback of the advection map.
    /// Once the data is retrieved, it extracts:
    /// - The fluid velocity (from red and green channels),
    /// - The buoyancy factor (from the blue channel),
    /// - And computes a drag force based on the object's velocity and the local fluid density.
    /// All forces are then applied to the Rigidbody2D.
    /// </summary>
    private void SampleFluidForce()
    {
        // Convert the object's world position into UV coordinates based on the fluid simulation's domain.
        Vector2 currentUV = WorldToUV(transform.position);

        // Begin asynchronous GPU readback.
        isAwaitingReadback = true;
        AsyncGPUReadback.Request(advectionMap, 0, TextureFormat.RGBA32, (AsyncGPUReadbackRequest request) =>
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error detected.");
                isAwaitingReadback = false;
                return;
            }

            // Retrieve the data as a NativeArray of Color32.
            var data = request.GetData<Color32>();
            int texWidth = advectionMap.width;
            int texHeight = advectionMap.height;

            // Convert our UV coordinate (0�1) to pixel coordinates.
            int pixelX = Mathf.Clamp((int)(currentUV.x * texWidth), 0, texWidth - 1);
            int pixelY = Mathf.Clamp((int)(currentUV.y * texHeight), 0, texHeight - 1);
            int index = pixelY * texWidth + pixelX;

            // Sample the color from the fetched data.
            Color32 color = data[index];

            // --- Compute Fluid Force (from R and G channels) ---
            float r = color.r / 255f;
            float g = color.g / 255f;
            float b = color.b / 255f;
            // Remap to -1 to 1.
            Vector2 fluidVelocity = new Vector2(r * 2f - 1f, g * 2f - 1f);
            float fluidDensity = b;
            Vector2 fluidForce = fluidVelocity * forceMultiplier * fluidDensity;
            Vector2 buoyancyForce = Vector2.up * fluidDensity * buoyancyMultiplier;

            // --- Compute Drag Force ---
            // The drag force opposes the object's current velocity.
            // It is scaled by dragMultiplier and modulated by the local fluid density (the blue channel value).
            Vector2 dragForce = -rb.linearVelocity * dragMultiplier * fluidDensity;

            // Apply the combined force to the Rigidbody2D.
            rb.AddForce(fluidForce + buoyancyForce + dragForce, ForceMode2D.Force);

            isAwaitingReadback = false;
        });
    }

    /// <summary>
    /// Converts a world-space position into UV coordinates (0-1) based on the fluid simulation's domain.
    /// The domain is obtained from the fluid manager's domainMin and domainMax.
    /// </summary>
    /// <param name="worldPosition">The world-space position to convert.</param>
    /// <returns>UV coordinates in the range [0,1].</returns>
    private Vector2 WorldToUV(Vector2 worldPosition)
    {
        if (fluidManager == null)
        {
            Debug.LogError("Fluid Manager is null, cannot compute UV. Returning (0,0).");
            return Vector2.zero;
        }

        // Build a Rect from the fluid manager's domain.
        Vector2 domainMin = fluidManager.domainMin;
        Vector2 domainMax = fluidManager.domainMax;
        float width = domainMax.x - domainMin.x;
        float height = domainMax.y - domainMin.y;

        // Map the world position into the 0-1 range.
        Vector2 uv;
        uv.x = (worldPosition.x - domainMin.x) / width;
        uv.y = (worldPosition.y - domainMin.y) / height;

        if (flipY)
            uv.y = 1f - uv.y;

        return uv;
    }
}
