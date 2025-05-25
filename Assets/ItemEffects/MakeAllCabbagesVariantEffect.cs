using UnityEngine;
using System.Collections.Generic;

public class MakeAllCabbagesVariantEffect : ItemEffect
{
    public CabbageVariantType variant;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }

        List<Cabbage> activeCabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        foreach (var c in activeCabbages)
        {
            c.SetVariant(variant);
        }
    }

    public override string GetDescription()
    {
        return ($"Make all cabbages {variant.ToString()}");
    }
}
