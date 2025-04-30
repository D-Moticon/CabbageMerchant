using UnityEngine;

public class ExtraLifeItemEffect : ItemEffect
{
    public int quantity = 1;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddLife(quantity);
    }

    public override string GetDescription()
    {
        if (quantity > 1)
        {
            return $"Grant {quantity} extra lives";
        }

        else
        {
            return $"Grant {quantity} extra life";
        }
    }

    public override void RandomizePower()
    {
        base.RandomizePower();
        quantity = Random.Range(1, 3);
    }
}
