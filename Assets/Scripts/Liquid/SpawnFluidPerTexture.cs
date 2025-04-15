using UnityEngine;

public class SpawnFluidPerTexture : MonoBehaviour
{
    private LiquidManager liquidManager;
    public Texture2D texture;
    public uint particleType;
    public int maxSpawnParticles = 1000;
    public float spawnScale = 1f;
    public float spawnThreshold = 0.5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        liquidManager = GameSingleton.Instance.liquidSim;
        liquidManager.SpawnParticlesFromTexture(this.transform.position, texture, maxSpawnParticles, particleType, spawnScale, spawnThreshold);
    }

}
