using UnityEngine;

public class IncreaseWeaponPower : ItemEffect
{
    public float weaponPowerLevelIncrease = 1;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.IncreaseWeaponPower(weaponPowerLevelIncrease);
    }

    public override string GetDescription()
    {
        return ($"Increase Weapon Power by {weaponPowerLevelIncrease:F0}");
    }
}
