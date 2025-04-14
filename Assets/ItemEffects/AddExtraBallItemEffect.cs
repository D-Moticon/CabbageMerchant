using UnityEngine;

public class AddExtraBallItemEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddExtraBall();
    }

    public override string GetDescription()
    {
        return ("Gain 1 extra ball");
    }
}
