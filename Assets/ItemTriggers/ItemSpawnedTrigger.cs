using UnityEngine;

public class ItemSpawnedTrigger : Trigger
{
    public bool normalItemOnly = true;
    public ItemSlot.AllowedTypes allowedTypes;
    
    public override void InitializeTrigger(Item item)
    {
        ItemManager.ItemSpawnedEvent += ItemSpawnedListener;
    }

    public override void RemoveTrigger(Item item)
    {
        ItemManager.ItemSpawnedEvent -= ItemSpawnedListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Any item spawned");
    }

    void ItemSpawnedListener(Item item)
    {
        if (allowedTypes == ItemSlot.AllowedTypes.itemOnly)
        {
            if (item.itemType != Item.ItemType.Item)
            {
                return;
            }
        }

        if (allowedTypes == ItemSlot.AllowedTypes.itemOrConsumable)
        {
            if (item.itemType != Item.ItemType.Item && item.itemType != Item.ItemType.Consumable)
            {
                return;
            }
        }
        
        TriggerContext tc = new TriggerContext();
        tc.itemA = item;
        owningItem.TryTriggerItem(tc);
    }
}
