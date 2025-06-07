using UnityEngine;

public class GameStateMachineStartedTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.GameStateMachineStartedAction += GameStateMachineStartedListener;
    }
    
    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.GameStateMachineStartedAction -= GameStateMachineStartedListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Start of round");
    }
    
    private void GameStateMachineStartedListener()
    {
        TriggerContext tc = new TriggerContext();
        owningItem.TryTriggerItem(tc);
    }
}
