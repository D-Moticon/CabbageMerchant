using System;
using UnityEngine;
using System.Collections.Generic;

public class SetSpriteBasedOnBiome : MonoBehaviour
{
    [System.Serializable]
    public class SpriteBiomePair
    {
        public Biome biome;
        public Sprite sprite;
    }

    public List<SpriteBiomePair> spriteBiomePairs;

    private void OnEnable()
    {
        if (Singleton.Instance.runManager.currentBiome == null)
        {
            return;
        }
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        for (int i = 0; i < spriteBiomePairs.Count; i++)
        {
            if (spriteBiomePairs[i].biome == Singleton.Instance.runManager.currentBiome)
            {
                sr.sprite = spriteBiomePairs[i].sprite;
            }
        }
    }
}
