using UnityEngine;

public class LauncherSpeedEffect : ItemEffect
{
    public float speedAdd = 5f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        GameSingleton.Instance.gameStateMachine.launcher.AddLaunchSpeed(speedAdd);
    }

    public override string GetDescription()
    {
        return ($"Increase ball launch speed by {speedAdd:F1} m/s");
    }
}
