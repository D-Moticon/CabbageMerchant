using UnityEngine;

public class ForceRoundFailEffect : ItemEffect
{
    public string roundFailMessage;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }
        
        GameSingleton.Instance.gameStateMachine.ForceRoundFail(roundFailMessage);
    }

    public override string GetDescription()
    {
        return ("Fail round");
    }
}
