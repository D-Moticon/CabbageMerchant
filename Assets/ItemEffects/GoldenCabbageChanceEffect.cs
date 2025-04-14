using UnityEngine;

public class GoldenCabbageChanceEffect : ItemEffect
{
    public float goldenChanceAdd = 0.02f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.goldenCabbageChance += goldenChanceAdd;
    }

    public override string GetDescription()
    {
        return($"Increase odds of golden cabbages spawning by {Helpers.ToPercentageString(goldenChanceAdd)}");
    }
}
