// GasFluidInput.cs
using UnityEngine;

public class GasFluidInput : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the GasFluidManager handling the simulation.")]
    public GasFluidManager fluidManager;

    [Header("Force Settings")]
    [Tooltip("Radius (in simulation grid cells) of the force area.")]
    public int forceRadius = 5;

    [Tooltip("Multiplier for converting mouse movement (pixels) into simulation force.")]
    public float forceStrength = 0.5f;

    [Tooltip("Amount of density to inject when applying force.")]
    public float forceDensity = 0.5f;

    public float injectionTemperature = 0.5f;

    [ColorUsage(true, true)]
    public Color injectionColor = Color.white;

    // For computing mouse velocity.
    private Vector3 lastMousePos;
    private bool hasLastMousePos = false;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePos = Input.mousePosition;
            if (!hasLastMousePos)
            {
                lastMousePos = currentMousePos;
                hasLastMousePos = true;
            }

            // Compute the mouse movement delta.
            Vector3 mouseDelta = currentMousePos - lastMousePos;
            lastMousePos = currentMousePos;

            // Convert the mouse's screen position to UV coordinates (0 to 1).
            Vector2 uv = new Vector2(currentMousePos.x / Screen.width, currentMousePos.y / Screen.height);
            // Map UV coordinates to simulation grid coordinates.
            Vector2 simPos = new Vector2(uv.x * fluidManager.gridWidth, uv.y * fluidManager.gridHeight);

            // Calculate the force vector from the mouse movement.
            Vector2 forceVel = new Vector2(mouseDelta.x, mouseDelta.y) * forceStrength;

            // Optional debug log to check the values.
            //Debug.Log($"Applying force at simPos: {simPos} with forceVel: {forceVel} and density: {forceDensity}");

            // Apply the force along with extra density.
            fluidManager.SpawnGasAtPosition(simPos, forceVel, forceRadius, forceDensity, injectionColor, injectionTemperature);
        }
        else
        {
            hasLastMousePos = false;
        }
    }
}
