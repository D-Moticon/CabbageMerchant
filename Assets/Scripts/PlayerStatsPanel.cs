using System;
using UnityEngine;
using TMPro;

public class PlayerStatsPanel : MonoBehaviour
{
    public TMP_Text runTimeText;
    public TMP_Text weaponPowerText;
    public TMP_Text weaponCooldownText;
    public TMP_Text shopDiscountText;
    public TMP_Text goldenCabbageText;
    public TMP_Text rareItemText;
    public TMP_Text legendaryItemText;
    public TMP_Text holofoilChanceText;

    private void OnEnable()
    {
        PlayerStats.WeaponPowerUpdatedEvent += WeaponPowerUpdatedListener;
        PlayerStats.WeaponCooldownSpeedUpdatedEvent += WeaponCooldownSpeedUpdatedListener;

        PlayerStats pStats = Singleton.Instance.playerStats;
        
        WeaponPowerUpdatedListener((int)Singleton.Instance.playerStats.weaponPowerLevel);
        WeaponCooldownSpeedUpdatedListener(Singleton.Instance.playerStats.weaponCooldownSpeedMult);
        var ts = System.TimeSpan.FromSeconds(pStats.totalRunTime);
        runTimeText.text = ts.ToString(@"mm\:ss\.f");
        goldenCabbageText.text = $"{Helpers.ToPercentageString(Singleton.Instance.playerStats.goldenCabbageChance)}";
        rareItemText.text = $"{Helpers.ToPercentageString(Singleton.Instance.playerStats.shopRarityMult*0.1f)}";
        legendaryItemText.text = $"{Helpers.ToPercentageString(Singleton.Instance.playerStats.shopRarityMult*0.02f)}";
        holofoilChanceText.text = $"{Helpers.ToPercentageString(Singleton.Instance.playerStats.holofoilChance)}";
        shopDiscountText.text = $"{Helpers.ToPercentageString(1f-Singleton.Instance.playerStats.shopDiscountMult)}";
    }

    private void OnDisable()
    {
        PlayerStats.WeaponPowerUpdatedEvent -= WeaponPowerUpdatedListener;
        PlayerStats.WeaponCooldownSpeedUpdatedEvent -= WeaponCooldownSpeedUpdatedListener;
    }
    
    private void WeaponPowerUpdatedListener(int value)
    {
        weaponPowerText.text = $"{value}";
    }
    
    private void WeaponCooldownSpeedUpdatedListener(float value)
    {
        weaponCooldownText.text = $"{value:F1}x";
    }
}
