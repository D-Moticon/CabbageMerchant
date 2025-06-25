using UnityEngine;

public class BallBonkValueItemEffect : ItemEffect
{
    public float valueAdd = 1f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }
        
        Ball b = null;
        if (tc != null)
        {
            b = tc.ball;
        }

        if (b == null)
        {
            b = GameSingleton.Instance.gameStateMachine.GetRandomActiveBall();
        }

        if (b == null)
        {
            return;
        }
        
        b.AddBonkValue(valueAdd);
        
    }

    public override string GetDescription()
    {
        return ($"Increase ball bonk value by +{valueAdd:F0}");
    }

    public override void RandomizePower()
    {
        base.RandomizePower();
        valueAdd = Random.Range(0.5f, 10f);
    }
}
