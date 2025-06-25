using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MultiplyEffectPowerOnRoundGoalExceeded : ItemEffect
{
    public double effectPowerMultMult = 2;
    public double goalMultThreshold = 2;
    private bool hasTriggeredThisRound = false;
    
    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        GameStateMachine.RoundGoalOverHitEvent += RoundGoalOverHitListener;
        GameStateMachine.ExitingScoringAction += ExitingScoringListener;
    }
    

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        GameStateMachine.RoundGoalOverHitEvent -= RoundGoalOverHitListener;
        GameStateMachine.ExitingScoringAction -= ExitingScoringListener;
    }
    
    private void RoundGoalOverHitListener(double mult)
    {
        if (hasTriggeredThisRound)
        {
            return;
        }
        
        if (mult >= goalMultThreshold)
        {
            List<ItemEffect> allEffects = new List<ItemEffect>(owningItem.effects);
            allEffects.AddRange(owningItem.holofoilEffects);
            
            List<ItemEffect> ies = allEffects
                .Where(x => x.GetType() != typeof(MultiplyEffectPowerOnRoundGoalExceeded))
                .ToList();
            
            foreach (var ie in ies)
            {
                ie.MultiplyPowerMult(effectPowerMultMult);
            }
            hasTriggeredThisRound = true;
        }
    }
    
    void ExitingScoringListener()
    {
        hasTriggeredThisRound = false;
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        //do nothing
    }

    public override string GetDescription()
    {
        return ($"This item's power increases by {effectPowerMultMult:F1}x if the round goal is exceeded by {goalMultThreshold:F0}x");
    }
}
