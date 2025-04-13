using UnityEngine;

public class FirstCabbageHitTrigger : Trigger
{
    bool hit = false;
        
    public override void InitializeTrigger(Item item)
    {
        Ball.BallHitCabbageEvent += CabbageHitListener;
        GameStateMachine.EnteringAimStateAction += AimStateEnteredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        Ball.BallHitCabbageEvent -= CabbageHitListener;
        GameStateMachine.EnteringAimStateAction -= AimStateEnteredListener;
    }

    public override string GetTriggerDescription()
    {
        return ("First cabbage bonked");
    }

    void CabbageHitListener(Ball.BallHitCabbageParams bcParams)
    {
        if (hit)
        {
            return;
        }

        TriggerContext tc = new TriggerContext();
        tc.ball = bcParams.ball;
        tc.cabbage = bcParams.cabbage;
        tc.point = bcParams.point;
        tc.normal = bcParams.normal;
        owningItem.TryTriggerItem(tc);
        hit = true;
            
    }

    void AimStateEnteredListener()
    {
        hit = false;
    }
}
