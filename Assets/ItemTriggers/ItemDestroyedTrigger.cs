using UnityEngine;
using System.Collections.Generic;

public class ItemDestroyedTrigger : Trigger
{
    public bool stopDestroy = false;
    public List<Item.ItemType> validItemTypes = new();
    
    public override void InitializeTrigger(Item item)
    {
        Item.DestroyItemPreEvent += OnItemDestroyed;
    }

    public override void RemoveTrigger(Item item)
    {
        Item.DestroyItemPreEvent -= OnItemDestroyed;
    }

    public override string GetTriggerDescription()
    {
        return $"When an item is destroyed (but not sold)";
    }

    void OnItemDestroyed(Item.DestroyItemParams dip)
    {
        if (dip.item == owningItem)
            return;

        if (dip.stopDestroy)
            return;

        if (validItemTypes.Count > 0 && !validItemTypes.Contains(dip.item.itemType))
            return;

        //merging items should be excluded
        if (!dip.isBeingSentToGraveyard)
            return;

        if (dip.isBeingSold)
            return;

        dip.stopDestroy = stopDestroy;
        owningItem.TryTriggerItem(new TriggerContext { itemA = dip.item });
    }

    public override void RandomizeTrigger() { }
}