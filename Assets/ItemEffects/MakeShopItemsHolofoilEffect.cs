using UnityEngine;

public class MakeShopItemsHolofoilEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        ShopManager[] sms = GameObject.FindObjectsByType<ShopManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (sms.Length == 0)
        {
            return;
        }

        ShopManager sm = sms[0];
        if (sm == null)
        {
            return;
        }

        foreach (Item item in sm.spawnedItems)
        {
            item.SetHolofoil();
        }
    }

    public override string GetDescription()
    {
        return ("Make all shop items holofoil");
    }
}
