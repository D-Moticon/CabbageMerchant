using UnityEngine;

public class TurnOnFloorItemEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        GameSingleton.Instance.gameStateMachine.TurnOnFloor();
    }

    public override string GetDescription()
    {
        return ("Balls can no longer fall through the bottom of the board");
    }
}
