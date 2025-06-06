using UnityEngine;

public class SetLauncherSpeed : ItemEffect
{
    public float newSpeed = 15f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        GameSingleton.Instance.gameStateMachine.launcher.SetLaunchSpeed(newSpeed);
    }

    public override string GetDescription()
    {
        return ($"Throw balls at {newSpeed} m/s");
    }
    
    public override void RandomizePower()
    {
        base.RandomizePower();
        newSpeed = Random.Range(12f, 40f);
    }
}
