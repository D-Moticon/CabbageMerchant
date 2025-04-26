using UnityEngine;
using System.Collections.Generic;

public class MultiplyRandomCabbageMultEffect : ItemEffect
{
    public int quantity = 1;
    public float multMult = 2f;

    public PooledObjectData vfx;
    public SFXInfo sfx;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        List<Cabbage> activeCabbagesShuffled =
            new List<Cabbage>(GameSingleton.Instance.gameStateMachine.activeCabbages);
        activeCabbagesShuffled.Shuffle();

        int finalQuantity = Mathf.Min(quantity, activeCabbagesShuffled.Count);
        
        for (int i = 0; i < finalQuantity; i++)
        {
            activeCabbagesShuffled[i].MultiplyBonkMultiplier(2f);
            activeCabbagesShuffled[i].PlayBonkFX();
            vfx.Spawn(activeCabbagesShuffled[i].transform.position);
            sfx.Play(activeCabbagesShuffled[i].transform.position);
        }
    }

    public override string GetDescription()
    {
        string plural = quantity>1? "s":"";
        string s = $"Multiply the bonk multiplier of {quantity} random cabbage{plural} by {multMult:F1}x";
        return s;
    }
}
