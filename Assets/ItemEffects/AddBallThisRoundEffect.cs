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
        string plural = "";
        if (quantity > 1) plural = "s";
        return ($"Gain {quantity} extra ball{plural} this round");
    }
}
