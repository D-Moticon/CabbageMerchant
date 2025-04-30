using UnityEngine;

public class ShopRemoveOnlyBuyOneEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        
    }

    public override string GetDescription()
    {
        return ("Allow multiple book reads in library");
    }

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        ShopManager.ShopEnteredEvent += OnShopEntered;
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        ShopManager.ShopEnteredEvent -= OnShopEntered;
    }

    void OnShopEntered(ShopManager shop)
    {
        shop.DisableOnlyBuyOne();
    }
}
