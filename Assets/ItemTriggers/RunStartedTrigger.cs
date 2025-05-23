using UnityEngine;

public class RunStartedTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        RunManager.RunStartEventLate += RunStartedListener;
    }

    public override void RemoveTrigger(Item item)
    {
        RunManager.RunStartEventLate -= RunStartedListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Run Started");
    }
    
    private void RunStartedListener(RunManager.RunStartParams rsp)
    {
        owningItem.TryTriggerItem();
    }
}
