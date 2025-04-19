using UnityEngine;

public class CabbageHitTrigger : Trigger
{
    public int everyXHit = 1;
    private int hitCounter = 0;
    public bool onlyByBall = false;
        
    public override void InitializeTrigger(Item item)
    {
        Cabbage.CabbageBonkedEvent += CabbageHitListener;
        GameStateMachine.EnteringAimStateAction += AimStateEnteredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        Cabbage.CabbageBonkedEvent -= CabbageHitListener;
        GameStateMachine.EnteringAimStateAction -= AimStateEnteredListener;
    }

    public override string GetTriggerDescription()
    {
        string plural = "";
        string onlyByBallString = "";
            
        if (everyXHit > 1)
        {
            plural = "s";
        }

        if (onlyByBall)
        {
            onlyByBallString = " by ball";
        }
            
        return ($"Every {everyXHit} cabbage{plural} bonked{onlyByBallString}");
    }

    void CabbageHitListener(BonkParams bp)
    {
        if (onlyByBall && !bp.treatAsBall)
        {
            return;
        }
            
        hitCounter++;
        if (hitCounter >= everyXHit)
        {
            TriggerContext tc = new TriggerContext();
            tc.ball = bp.ball;
            tc.cabbage = bp.bonkedCabbage;
            tc.point = bp.collisionPos;
            tc.normal = bp.normal;
            owningItem.TryTriggerItem(tc);
            hitCounter = 0;
        }
    }

    void AimStateEnteredListener()
    {
        hitCounter = 0;
    }
}