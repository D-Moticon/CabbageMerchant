using UnityEngine;

public class ItemSlotChangedTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        ItemManager.ItemAddedToSlotEvent += ItemAddedToSlotListener;
    }
    
    public override void RemoveTrigger(Item item)
    {
        ItemManager.ItemAddedToSlotEvent -= ItemAddedToSlotListener;
    }

    public override string GetTriggerDescription()
    {
        return "Item slot changed";
    }
    
    private void ItemAddedToSlotListener(Item item, ItemSlot slot)
    {
        if (item == owningItem)
        {
            TriggerContext tc = new TriggerContext();
            tc.itemA = owningItem;
            owningItem.TryTriggerItem(tc);
        }
    }
}
