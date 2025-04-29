using UnityEngine;
using System.Collections.Generic;

public class BonkLowestCabbageEffect : ItemEffect
{
    [Tooltip("Amount of bonk damage to deal to the weakest cabbage")]
    public float bonkValue = 1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        var activeCabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        if (activeCabbages == null || activeCabbages.Count == 0)
            return;

        // Find the cabbage with the lowest points
        Cabbage lowest = null;
        double minPoints = double.MaxValue;
        foreach (var c in activeCabbages)
        {
            if (c == null) continue;
            if (c.points < minPoints)
            {
                minPoints = c.points;
                lowest = c;
            }
        }

        if (lowest == null) 
            return;

        // Attempt to bonk it (assuming it implements IBonkable)
        if (lowest.TryGetComponent<IBonkable>(out var bonkable))
        {
            BonkParams bp = new BonkParams();
            bp.bonkerPower = bonkValue;
            bonkable.Bonk(bp);
        }
    }

    public override string GetDescription()
    {
        return $"Bonk the lowest value cabbage for {bonkValue}.";
    }
    
    public override void RandomizePower()
    {
        base.RandomizePower();
        bonkValue = Random.Range(0.5f, 8f);
    }
}