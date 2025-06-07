using UnityEngine;

public class SellValueEffect : ItemEffect
{
    public float sellValueIncrease = 1f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        owningItem.SetNormalizedPrice(owningItem.normalizedPrice + sellValueIncrease*2f/Item.globalItemPriceMult);
    }

    public override string GetDescription()
    {
        return ($"Increase sell value by {sellValueIncrease:F0}");
    }

    public override void RandomizePower()
    {
        base.RandomizePower();
        sellValueIncrease = Random.Range(1, 3);
    }
}
