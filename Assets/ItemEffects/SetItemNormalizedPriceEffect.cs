using UnityEngine;

public class SetItemNormalizedPriceEffect : ItemEffect
{
    public float normalizedPrice = 0.5f;
    public bool dontEffectFreeItems = true;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc.itemA == null)
        {
            return;
        }

        if (dontEffectFreeItems && tc.itemA.normalizedPrice <= 0f)
        {
            return;
        }
        
        tc.itemA.SetNormalizedPrice(normalizedPrice);
    }

    public override string GetDescription()
    {
        return ($"Set item price to {normalizedPrice * Item.globalItemPriceMult}");
    }
}
