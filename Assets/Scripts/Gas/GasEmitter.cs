using UnityEngine;

public class GasEmitter : MonoBehaviour
{
    public float emissionRadius = 5f;
    public float emissionDensity = 0.7f;
    public float emissionTemperature = 0f;
    [ColorUsage(true, true)]
    public Color emissionColor = Color.white;
    public float velocityInherit = 0f;
    private Vector2 lastPos;
    GasFluidManager fluidManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fluidManager = GameSingleton.Instance.gasSim;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentPos = this.transform.position;

        // Compute the mouse movement delta.
        Vector2 posDelta = currentPos - lastPos;
        lastPos = currentPos;

        // Calculate the force vector from the mouse movement.
        Vector2 forceVel = new Vector2(posDelta.x, posDelta.y) * velocityInherit;

        // Optional debug log to check the values.
        //Debug.Log($"Applying force at simPos: {simPos} with forceVel: {forceVel} and density: {forceDensity}");

        // Apply the force along with extra density.
        fluidManager.SpawnGasAtPosition(currentPos, forceVel, emissionRadius, emissionDensity*Time.deltaTime, emissionColor, emissionTemperature);
    }
}
