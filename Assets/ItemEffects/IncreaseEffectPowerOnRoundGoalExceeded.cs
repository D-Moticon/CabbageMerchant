using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IncreaseEffectPowerOnRoundGoalExceeded : ItemEffect
{
    public float effectPowerMultAdd = 0.5f;
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
                .Where(x => x.GetType() != typeof(IncreaseEffectPowerOnRoundGoalExceeded))
                .ToList();

            foreach (var ie in ies)
            {
                ie.IncreasePowerMult(effectPowerMultAdd);
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
        return ($"This item's power increases by {Helpers.ToPercentageString(effectPowerMultAdd)} of base power if the round goal is exceeded by {goalMultThreshold:F0}x");
    }
}
