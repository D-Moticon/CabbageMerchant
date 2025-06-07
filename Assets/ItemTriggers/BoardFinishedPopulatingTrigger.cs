using UnityEngine;

public class BoardFinishedPopulatingTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.BoardFinishedPopulatingAction += BoardFinishedPopulatingListener;
        Item.ItemUnFrozenEvent += ItemUnFrozenListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.BoardFinishedPopulatingAction -= BoardFinishedPopulatingListener;
        Item.ItemUnFrozenEvent -= ItemUnFrozenListener;
    }

    public override string GetTriggerDescription()
    {
        return ("At start of round");
    }

    void BoardFinishedPopulatingListener()
    {
        owningItem.TryTriggerItem();
    }
    
    private void ItemUnFrozenListener(Item item)
    {
        if (!item.triggeredThisBoard)
        {
            owningItem.TryTriggerItem();
        }
    }
}
