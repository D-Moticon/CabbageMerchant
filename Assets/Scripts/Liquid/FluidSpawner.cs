using UnityEngine;

public class FluidSpawner2D : MonoBehaviour
{
    public Camera mainCamera;
    public uint particleType = 1;
    public float spawnVelocity = 2f;
    public int particlesPerSecond = 100;
    public float spawnRadius = 0.5f;

    void Update()
    {
        if (Input.GetMouseButton(0)) // Left-click
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            int particlesToSpawn = Mathf.RoundToInt(particlesPerSecond * Time.deltaTime);
            if (particlesToSpawn == 0)
            {
                particlesToSpawn = 1;
            }

            for (int i = 0; i < particlesToSpawn; i++)
            {
                // Random small offset
                Vector2 spawnPos = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                // You can also randomize velocity if you�d like
                Vector2 vel = Random.insideUnitCircle * spawnVelocity;

                GameSingleton.Instance.liquidSim.SpawnParticle(spawnPos, vel, particleType, spawnRadius);
            }
        }
    }
}
