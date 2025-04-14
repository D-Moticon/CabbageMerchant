using UnityEngine;

public class ShopRarityEffect : ItemEffect
{
    public float rarityMultAdd = 0.5f;
    public string rarityMultAdjective = "by some";
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddShopRarityMult(rarityMultAdd);
    }

    public override string GetDescription()
    {
        return($"Improve odds of finding rare items in shops {rarityMultAdjective}");
    }
}
