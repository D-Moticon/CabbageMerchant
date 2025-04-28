using UnityEngine;

public class ItemInSlotTriggered : Trigger
{
    public int quantity = 1;
    private int currentCount = 0;
    private static Vector2Int randomizeQuantityRange = new Vector2Int(1, 7);
    public int slotForTrigger = 0;
    
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

        return ($"Item in slot {slotForTrigger+1} triggered {quantity} time{plural} (cannot trigger self)");
    }

    void ItemTriggeredListener(Item item)
    {
        
        if (item == owningItem)
        {
            //prevent infinite loops
            return;
        }

        if (!Singleton.Instance.itemManager.itemSlots.Contains(item.currentItemSlot))
        {
            return;
        }

        if (Singleton.Instance.itemManager.itemSlots.IndexOf(item.currentItemSlot) != slotForTrigger)
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
        slotForTrigger = Random.Range(0, Singleton.Instance.itemManager.itemSlots.Count);
    }
}