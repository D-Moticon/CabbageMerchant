using UnityEngine;

public class BallBonkValueItemEffect : ItemEffect
{
    public float valueAdd = 1f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc != null && tc.ball != null)
        {
            tc.ball.AddBonkValue(valueAdd);
        }
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
