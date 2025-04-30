using UnityEngine;

public class ForceNextBallRainbowEffect : ItemEffect
{
    public float bonkValueIncrease = 1f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.launchModifierManager.AddNextBallValue(bonkValueIncrease);
    }

    public override string GetDescription()
    {
        return ($"Next launched ball bonk value +{bonkValueIncrease:F1}");
    }
}
