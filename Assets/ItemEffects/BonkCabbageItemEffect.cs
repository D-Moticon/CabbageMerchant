using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class BonkCabbageItemEffect : ItemEffect
{
    public float bonkValue = 1f;
    public int quantity = 1;

    public enum CabbageSelection
    {
        random,
        lowest,
        highest,
        bonked
    }

    public CabbageSelection cabbageSelection;
    public PooledObjectData spawnItemAtCabbage;
    public Sprite spriteForFloater;
    public bool displayBonkValueInFloater = true;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        List<Cabbage> cabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;

        if (cabbages == null || cabbages.Count == 0)
        {
            return; // no cabbages to bonk
        }

        List<Cabbage> selectedCabbages = new List<Cabbage>();

        switch (cabbageSelection)
        {
            case CabbageSelection.random:
                selectedCabbages = cabbages.OrderBy(x => UnityEngine.Random.value).Take(quantity).ToList();
                break;

            case CabbageSelection.lowest:
                selectedCabbages = cabbages.OrderBy(c => c.sizeLevel).Take(quantity).ToList();
                break;

            case CabbageSelection.highest:
                selectedCabbages = cabbages.OrderByDescending(c => c.sizeLevel).Take(quantity).ToList();
                break;
            case CabbageSelection.bonked:
                if (tc.cabbage == null)
                {
                    return;
                }
                selectedCabbages.Add(tc.cabbage);
                break;
            default:
                return;
        }

        foreach (var c in selectedCabbages)
        {
            double finalBonkPower = bonkValue * powerMult;
            BonkParams bp = new BonkParams();
            bp.bonkerPower = finalBonkPower;
            bp.collisionPos = c.transform.position;
            c.Bonk(bp);

            string floaterString = "";
            if (displayBonkValueInFloater)
            {
                floaterString = $"{finalBonkPower:F1}";
            }
            
            if (spawnItemAtCabbage != null)
            {
                spawnItemAtCabbage.Spawn(c.transform.position);
            }

            if (spriteForFloater != null)
            {
                Singleton.Instance.floaterManager.SpawnSpriteFloater(floaterString, c.transform.position, spriteForFloater, Color.white, 1f, owningItem.isHolofoil);
            }
        }
    }

    public override string GetDescription()
    {
        if (cabbageSelection == CabbageSelection.bonked)
        {
            return($"Bonk cabbage for additional {bonkValue*powerMult:F1} points");
        }
        
        string selectionDescription = cabbageSelection switch
        {
            CabbageSelection.random => "random",
            CabbageSelection.lowest => "lowest",
            CabbageSelection.highest => "highest",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return ($"Bonk {quantity} {selectionDescription} cabbage(s) for {bonkValue*powerMult:F1}");
    }
    
    public override void RandomizePower()
    {
        base.RandomizePower();
        powerMult = (double)Random.Range(0.5f, 10f);
        //bonkValue = Random.Range(0.5f, 10f);
        quantity = Random.Range(1, 8);
    }
}