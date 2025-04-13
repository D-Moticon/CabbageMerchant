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
        if (purchasedItem == owningItem)
        {
            Debug.Log("MEOW");
            alreadyAdded = true;
            owningItem.TryTriggerItem();
        }
        
    }
}
