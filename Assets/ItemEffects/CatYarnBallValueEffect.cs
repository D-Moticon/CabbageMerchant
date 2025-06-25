using UnityEngine;

public class CatYarnBallValueEffect : ItemEffect
{
    public double valueAdd = 1;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddYarnBallPointAdd(valueAdd);
    }

    public override string GetDescription()
    {
        return ($"Yarn ball starts with +{valueAdd} bonk multiplier");
    }
}
