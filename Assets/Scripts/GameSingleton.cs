using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class GameSingleton : MonoBehaviour, IBiomeChangeable
{
    public static GameSingleton Instance { get; private set; }

    public ObjectPoolManager objectPoolManager;
    public GameStateMachine gameStateMachine;
    public BoardMetrics boardMetrics;
    public Transform gameSceneParent;
    public GasFluidManager gasSim;
    public LiquidManager liquidSim;
    public FluidRTReferences fluidRTReferences;

    [System.Serializable]
    public class BiomeInfo
    {
        public Biome biome;
        public BiomeParent biomeParent;
    }

    public List<BiomeInfo> biomeInfos;
    [HideInInspector]public BiomeParent currentBiomeParent;
    
    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        SetBiome(Singleton.Instance.runManager.currentBiome);
    }

    public void SetBiome(Biome biome)
    {
        for (int i = 0; i < biomeInfos.Count; i++)
        {
            if (biomeInfos[i].biome == biome)
            {
                biomeInfos[i].biomeParent.gameObject.SetActive(true);
                currentBiomeParent = biomeInfos[i].biomeParent;
            }

            else
            {
                biomeInfos[i].biomeParent.gameObject.SetActive(false);
            }
        }
    }
}
