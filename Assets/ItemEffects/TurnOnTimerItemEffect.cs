using UnityEngine;

public class TurnOnTimerItemEffect : ItemEffect
{
    public float timerDuration = 7f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        GameSingleton.Instance.gameStateMachine.SetTimerMode(true, timerDuration);
    }

    public override string GetDescription()
    {
        return ($"Destroy all balls after {timerDuration:F1}s");
    }
}
