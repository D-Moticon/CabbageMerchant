using UnityEngine;

public class ShopItemsDiscountAndTemporaryEffect : ItemEffect
{
    public float priceMult = 0.5f;
    public int tempNumber = 10;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
       
    }

    public override string GetDescription()
    {
        return ($"Shop items cost 50% less but only last 10 shots");
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
        
        item.MultiplyNormalizedPrice(priceMult);
        item.MakeTemporary(tempNumber);
    }
}
