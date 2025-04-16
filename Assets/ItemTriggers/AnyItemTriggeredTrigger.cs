using UnityEngine;

public class AnyItemTriggeredTrigger : Trigger
{
    public int quantity = 1;
    private int currentCount = 0;
    
    public override void InitializeTrigger(Item item)
    {
        Item.ItemTriggeredEvent += ItemTriggeredListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        Item.ItemTriggeredEvent -= ItemTriggeredListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
    }

    public override string GetTriggerDescription()
    {
        string plural = "";
        if (quantity > 1)
        {
            plural = "s";
        }

        return ($"{quantity} item{plural} triggered");
    }

    void ItemTriggeredListener(Item item)
    {
        if (item == owningItem)
        {
            //prevent infinite loops
            return;
        }
        currentCount++;
        if (currentCount >= quantity)
        {
            owningItem.TryTriggerItem();
            currentCount = 0;
        }
    }

    void BallFiredListener(Ball b)
    {
        currentCount = 0;
    }
}
