using UnityEngine;

public class ItemSpawnedTrigger : Trigger
{
    public bool normalItemOnly = true;
    
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
        if (normalItemOnly)
        {
            if (item.itemType != Item.ItemType.Item)
            {
                return;
            }
        }
        
        TriggerContext tc = new TriggerContext();
        tc.itemA = item;
        owningItem.TryTriggerItem(tc);
    }
}
