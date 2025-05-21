using UnityEngine;

public class AnyItemTriggeredTrigger : Trigger
{
    public int quantity = 1;
    private int currentCount = 0;
    private static Vector2Int randomizeQuantityRange = new Vector2Int(1, 7);
    
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

        return ($"{quantity} item{plural} triggered (excluding self)");
    }

    void ItemTriggeredListener(Item item)
    {
        if (item == owningItem)
        {
            //prevent infinite loops
            return;
        }

        if (itemHasTriggeredThisFrame)
        {
            return;
        }
        
        if (GameSingleton.Instance == null)
        {
            return;
        }
        
        if (!(GameSingleton.Instance.gameStateMachine.currentState is GameStateMachine.BouncingState))
        {
            //Prevent triggering from board populated items
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

    public override void RandomizeTrigger()
    {
        quantity = Random.Range(randomizeQuantityRange.x, randomizeQuantityRange.y);
    }
}
