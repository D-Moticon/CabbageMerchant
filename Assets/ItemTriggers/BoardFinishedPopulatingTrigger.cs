using UnityEngine;

public class BoardFinishedPopulatingTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.BoardFinishedPopulatingAction += BoardFinishedPopulatingListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.BoardFinishedPopulatingAction -= BoardFinishedPopulatingListener;
    }

    public override string GetTriggerDescription()
    {
        return ("At start of round");
    }

    void BoardFinishedPopulatingListener()
    {
        owningItem.TryTriggerItem();
    }
}
