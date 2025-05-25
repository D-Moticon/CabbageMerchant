using UnityEngine;

public class ItemAddedTrigger : Trigger
{
    private bool alreadyAdded = false;
    
    public override void InitializeTrigger(Item item)
    {
        ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
    }

    public override void RemoveTrigger(Item item)
    {
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
    }

    public override string GetTriggerDescription()
    {
        return "";
    }

    void ItemPurchasedListener(Item purchasedItem)
    {
        if (alreadyAdded)
        {
            return;
        }

        TriggerContext tc = new TriggerContext();
        tc.itemA = purchasedItem;
        
        if (purchasedItem == owningItem)
        {
            alreadyAdded = true;
            owningItem.TryTriggerItem(tc);
        }
        
    }
}
