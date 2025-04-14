using UnityEngine;

public class DiscountItemEffect : ItemEffect
{
    public float discountAmount = 0.1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddShopDiscount(discountAmount);
    }

    public override string GetDescription()
    {
        return ($"Shop items cost {Helpers.ToPercentageString(discountAmount)} less");
    }
}
