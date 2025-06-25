using UnityEngine;

public class KeyChanceEffect : ItemEffect
{
    public float keyChanceAdd = 0.1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddKeyChance(keyChanceAdd);
    }

    public override string GetDescription()
    {
        return ($"Increase chances of keys spawning by {Helpers.ToPercentageString(keyChanceAdd)}");
    }
}
