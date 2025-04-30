using UnityEngine;

public class ItemSoldTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        ItemManager.ItemSoldEvent += ItemSoldListener;
    }

    public override void RemoveTrigger(Item item)
    {
        ItemManager.ItemSoldEvent -= ItemSoldListener;
    }

    public override string GetTriggerDescription()
    {
        return "This item eaten";
    }

    void ItemSoldListener(Item item)
    {
        if (item == owningItem)
        {
            owningItem.TryTriggerItem();
        }
    }
}
