using UnityEngine;

public class ItemSpawnedInShopTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        ShopManager.ItemSpawnedInShopEvent += ItemSpawnedInShopListener;
    }

    public override void RemoveTrigger(Item item)
    {
        ShopManager.ItemSpawnedInShopEvent -= ItemSpawnedInShopListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Any item spawned In shop");
    }

    void ItemSpawnedInShopListener(Item item)
    {
        TriggerContext tc = new TriggerContext();
        tc.itemA = item;
        owningItem.TryTriggerItem(tc);
    }
}
