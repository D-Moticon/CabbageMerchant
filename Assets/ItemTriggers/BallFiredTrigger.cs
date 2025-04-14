using UnityEngine;

public class BallFiredTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.BallFiredEvent += BallFiredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.BallFiredEvent -= BallFiredListener;
    }

    public override string GetTriggerDescription()
    {
        return ("On ball fired");
    }

    void BallFiredListener(Ball b)
    {
        TriggerContext tc = new TriggerContext();
        tc.ball = b;
        owningItem.TryTriggerItem(tc);
    }
}
