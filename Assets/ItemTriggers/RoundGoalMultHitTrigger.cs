using UnityEngine;

public class RoundGoalMultHitTrigger : Trigger
{
    public double goalMultThreshold = 2;
    private bool hasTriggeredThisRound = false;
    
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.RoundGoalOverHitEvent += RoundGoalListener;
        GameStateMachine.ExitingScoringAction += ExitingScoringListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.RoundGoalOverHitEvent -= RoundGoalListener;
        GameStateMachine.ExitingScoringAction -= ExitingScoringListener;
    }

    public override string GetTriggerDescription()
    {
        return ($"Round goal exceeded by {goalMultThreshold}x");
    }

    void RoundGoalListener(double mult)
    {
        if (hasTriggeredThisRound)
        {
            return;
        }
        
        if (mult >= goalMultThreshold)
        {
            owningItem.TryTriggerItem();
            hasTriggeredThisRound = true;
        }
    }

    void ExitingScoringListener()
    {
        hasTriggeredThisRound = false;
    }
    
}
