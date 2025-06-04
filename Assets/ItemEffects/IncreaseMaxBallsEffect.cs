using UnityEngine;

public class IncreaseMaxBallsEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.IncreaseMaxBalls();
    }

    public override string GetDescription()
    {
        return ("Max Balls +1");
    }
}
