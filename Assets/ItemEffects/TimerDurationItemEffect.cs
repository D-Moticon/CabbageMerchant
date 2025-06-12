using UnityEngine;

public class TimerDurationItemEffect : ItemEffect
{
    public float timeAdd = 1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddTimerDuration(timeAdd);
    }

    public override string GetDescription()
    {
        return ($"Add {timeAdd:F1} seconds to shot timer");
    }
}
