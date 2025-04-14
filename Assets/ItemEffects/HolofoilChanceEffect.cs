using UnityEngine;

public class HolofoilChanceEffect : ItemEffect
{
    public float holofoilChanceAdd = 0.01f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddHolofoilChance(holofoilChanceAdd);
    }

    public override string GetDescription()
    {
        return ($"Increase odds of holofoil items by {Helpers.ToPercentageString(holofoilChanceAdd)}");
    }
}
