using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BonkCabbageItemEffect : ItemEffect
{
    public float bonkValue = 1f;
    public int quantity = 1;

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
        }

        foreach (var c in selectedCabbages)
        {
            BonkParams bp = new BonkParams();
            bp.bonkValue = bonkValue;
            bp.collisionPos = c.transform.position;
            c.Bonk(bp);

            if (spawnItemAtCabbage != null)
            {
                spawnItemAtCabbage.Spawn(c.transform.position);
            }
        }
    }

    public override string GetDescription()
    {
        string selectionDescription = cabbageSelection switch
        {
            CabbageSelection.random => "random",
            CabbageSelection.lowest => "lowest",
            CabbageSelection.highest => "highest",
            _ => throw new ArgumentOutOfRangeException()
        };

        return ($"Bonk {quantity} {selectionDescription} cabbage(s) for {bonkValue}");
    }
}