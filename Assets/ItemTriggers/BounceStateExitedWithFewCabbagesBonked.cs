using UnityEngine;

public class BounceStateExitedWithFewCabbagesBonked : Trigger
{
    public int cabbagesBonkedThresholdExclusive = 3;
    private int cabbagesBonkedCounter;
    
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.ExitingBounceStateEarlyAction += ExitedBounceStateEarlyListener;
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
        cabbagesBonkedCounter = 0;
    }

    

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.ExitingBounceStateEarlyAction -= ExitedBounceStateEarlyListener;
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
    }

    public override string GetTriggerDescription()
    {
        return ($"Fewer than {cabbagesBonkedThresholdExclusive} cabbages bonked in a turn");
    }
    
    private void CabbageBonkedListener(BonkParams bp)
    {
        if (bp.ball == null)
        {
            return;
        }

        cabbagesBonkedCounter++;
    }
    
    private void ExitedBounceStateEarlyListener()
    {
        if (cabbagesBonkedCounter < cabbagesBonkedThresholdExclusive)
        {
            TriggerContext tc = new TriggerContext();
            tc.itemA = owningItem;
            owningItem.TryTriggerItem(tc);
        }
        
        cabbagesBonkedCounter = 0;
    }
}
