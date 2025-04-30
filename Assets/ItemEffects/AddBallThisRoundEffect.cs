using UnityEngine;

public class AddBallThisRoundEffect : ItemEffect
{
    public int quantity = 1;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }
        GameSingleton.Instance.gameStateMachine.AddExtraBall(quantity);
    }

    public override string GetDescription()
    {
        return ($"Gain 1 extra ball this round");
    }
}
