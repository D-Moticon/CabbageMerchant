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
        
        Singleton.Instance.itemManager.UpgradeItem(item);
    }
}
