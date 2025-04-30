using UnityEngine;

public class BallFiredEarlyTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.BallFiredEarlyEvent += BallFiredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.BallFiredEarlyEvent -= BallFiredListener;
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
