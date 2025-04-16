using UnityEngine;

public class CabbageMergedTrigger : Trigger
{
    public int quantity = 1;
    private int counter = 0;
    
    public override void InitializeTrigger(Item item)
    {
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
    }

    public override string GetTriggerDescription()
    {
        string plural = quantity > 0 ? "" : "s";
        return ($"{quantity} cabbage{plural} merged");
    }

    void CabbageMergedListener(Cabbage.CabbageMergedParams cmp)
    {
        counter++;
        if (counter >= quantity)
        {
            owningItem.TryTriggerItem();
            counter = 0;
        }
    }
    
    void BallFiredListener(Ball b)
    {
        //counter = 0;
    }
}
