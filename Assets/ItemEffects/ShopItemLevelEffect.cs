using UnityEngine;

public class ShopItemLevelEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
       
    }

    public override string GetDescription()
    {
        return ($"Shop items are 1 level higher");
    }

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        ShopManager.ItemSpawnedInShopEvent += ItemSpawnedInShopListener;
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        ShopManager.ItemSpawnedInShopEvent -= ItemSpawnedInShopListener;
    }
    
    private void ItemSpawnedInShopListener(Item item)
    {
        if (item == null)
        {
            //This could happen with multiple of these items for instance
            return;
        }
        
        Item upItem = Singleton.Instance.itemManager.UpgradeItem(item);
        
        //Need to fire the event to (for instance) make the mystery CC give the correct price
        //But need to not fire this script by itself
        if (upItem != null)
        {
            ShopManager.ItemSpawnedInShopEvent -= ItemSpawnedInShopListener;
            ShopManager.ItemSpawnedInShopEvent?.Invoke(upItem);
            ShopManager.ItemSpawnedInShopEvent += ItemSpawnedInShopListener;
        }
        
    }
}
