using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class ActivateGameobjectBasedOnBiome : MonoBehaviour
{
    [System.Serializable]
    public class ObjectBiomePair
    {
        public Biome biome;
        public GameObject go;
    }

    public List<ObjectBiomePair> goBiomePairs;

    public enum ActivationType
    {
        onEnable,
        onBiomeChanged
    }

    public ActivationType activationType;

    private void OnEnable()
    {
        switch (activationType)
        {
            case ActivationType.onEnable:
                break;
            case ActivationType.onBiomeChanged:
                RunManager.BiomeChangedEvent += BiomeChangedListener;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (Singleton.Instance.runManager.currentBiome == null)
        {
            return;
        }
        
        for (int i = 0; i < goBiomePairs.Count; i++)
        {
            if (goBiomePairs[i].biome == Singleton.Instance.runManager.currentBiome)
            {
                goBiomePairs[i].go.SetActive(true);
            }
            else
            {
                goBiomePairs[i].go.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        RunManager.BiomeChangedEvent -= BiomeChangedListener;
    }

    void BiomeChangedListener(Biome b)
    {
        for (int i = 0; i < goBiomePairs.Count; i++)
        {
            if (goBiomePairs[i].biome == b)
            {
                goBiomePairs[i].go.SetActive(true);
            }
            else
            {
                goBiomePairs[i].go.SetActive(false);
            }
        }
    }
}
