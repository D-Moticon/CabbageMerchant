using UnityEngine;

public class RoundGoalMissedTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.RoundFailedEvent += RoundFailedListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.RoundFailedEvent -= RoundFailedListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Round Goal Missed");
    }
    
    private void RoundFailedListener()
    {
        owningItem.TryTriggerItem();
    }

}
