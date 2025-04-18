using UnityEngine;

public class WeaponCooldownStatEffect : ItemEffect
{
    public float cooldownMultAdd = 0.1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddWeaponCooldownSpeedMult(cooldownMultAdd);
    }

    public override string GetDescription()
    {
        return ($"Reduce weapon cooldowns by {Helpers.ToPercentageString(cooldownMultAdd)}");
    }
}
