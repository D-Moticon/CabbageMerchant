using System;
using UnityEngine;
using System.Collections.Generic;

public class BonkCabbageItemEffect : ItemEffect
{
    public float bonkValue = 1f;

    public enum CabbageSelection
    {
        random,
        lowest,
        highest
    }

    public CabbageSelection cabbageSelection;
    public PooledObjectData spawnItemAtCabbage;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        List<Cabbage> cabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        
        if (cabbages == null || cabbages.Count == 0)
        {
            return; // no cabbages to bonk
        }

        Cabbage c = null;

        switch (cabbageSelection)
        {
            case CabbageSelection.random:
            {
                int index = UnityEngine.Random.Range(0, cabbages.Count);
                c = cabbages[index];
                break;
            }
            case CabbageSelection.lowest:
            {
                // Sort or iterate to find the minimum sizeLevel
                Cabbage lowest = cabbages[0];
                for (int i = 1; i < cabbages.Count; i++)
                {
                    if (cabbages[i].sizeLevel < lowest.sizeLevel)
                    {
                        lowest = cabbages[i];
                    }
                }
                c = lowest;
                break;
            }
            case CabbageSelection.highest:
            {
                // Sort or iterate to find the maximum sizeLevel
                Cabbage highest = cabbages[0];
                for (int i = 1; i < cabbages.Count; i++)
                {
                    if (cabbages[i].sizeLevel > highest.sizeLevel)
                    {
                        highest = cabbages[i];
                    }
                }
                c = highest;
                break;
            }
        }

        if (c != null)
        {
            // Bonk method that takes bonkValue plus collision position
            // If your Bonk signature is Bonk(Vector2 collisionPos),
            // adapt as needed.
            c.Bonk(bonkValue, c.transform.position);

            if (spawnItemAtCabbage != null)
            {
                spawnItemAtCabbage.Spawn(c.transform.position);
            }
        }
    }

    public override string GetDescription()
    {
        switch (cabbageSelection)
        {
            case CabbageSelection.random:
                return ($"Bonk 1 random cabbage for {bonkValue}");
            case CabbageSelection.lowest:
                return ($"Bonk 1 lowest cabbage for {bonkValue}");
            case CabbageSelection.highest:
                return ($"Bonk 1 highest cabbage for {bonkValue}");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
