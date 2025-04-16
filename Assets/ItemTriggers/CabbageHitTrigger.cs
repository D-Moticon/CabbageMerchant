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

    void CabbageHitListener(Cabbage.CabbageBonkParams cbp)
    {
        if (onlyByBall && !cbp.treatAsBall)
        {
            return;
        }
            
        hitCounter++;
        if (hitCounter >= everyXHit)
        {
            TriggerContext tc = new TriggerContext();
            tc.ball = cbp.ball;
            tc.cabbage = cbp.c;
            tc.point = cbp.pos;
            tc.normal = cbp.normal;
            owningItem.TryTriggerItem(tc);
            hitCounter = 0;
        }
    }

    void AimStateEnteredListener()
    {
        hitCounter = 0;
    }
}