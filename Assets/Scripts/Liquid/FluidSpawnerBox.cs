using UnityEngine;

public class FluidSpawnerBox : MonoBehaviour
{
    public uint particleType = 1;
    public Vector2 boxSize = new Vector2(1f, 1f);
    public Vector2 velocity = Vector2.zero;
    public int particlesPerSecond = 500;

    private void Update()
    {
        int particlesToSpawn = Mathf.RoundToInt(particlesPerSecond * Time.deltaTime);
        if (particlesToSpawn == 0)
        {
            particlesToSpawn = 1;
        }

        for (int i = 0; i < particlesToSpawn; i++)
        {
            Vector2 rand = new Vector2(Random.Range(-boxSize.x / 2f, boxSize.x / 2f), Random.Range(-boxSize.y / 2f, boxSize.y / 2f));
            Vector2 pos = (Vector2)this.transform.position + rand;

            GameSingleton.Instance.liquidSim.SpawnParticle(pos, velocity, particleType, 0.5f, true);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(this.transform.position, boxSize);
    }
}
