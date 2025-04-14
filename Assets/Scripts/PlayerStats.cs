using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public double startingCoins = 0;
    
    public int startingBalls = 3;
    [HideInInspector]public int currentBalls = 3;
    [HideInInspector] public int extraStartingCabbages = 0;
    
    [HideInInspector]public double coins;
    public float startingHolofoilChance = 0.005f;
    [HideInInspector] public float holofoilChance = 0.005f;
    public float startingGoldenCabbageChance = 0.01f;
    [HideInInspector] public float goldenCabbageChance = 0.01f;
    public float startingGoldenCabbageValue = 1f;
    [HideInInspector] public float goldenCabbageValue = 1f;
    [HideInInspector] public float shopDiscountMult = 1f;
    [HideInInspector] public float shopRarityMult = 1f;
    
    public delegate void DoubleEvent(double value);

    public static DoubleEvent CoinsUpdated;

    private void OnEnable()
    {
        RunManager.RunStartEvent += StartRunListener;
    }

    private void OnDisable()
    {
        RunManager.RunStartEvent -= StartRunListener;
    }

    void StartRunListener(RunManager.RunStartParams rsp)
    {
        coins = startingCoins;
        CoinsUpdated?.Invoke(coins);
        
        currentBalls = startingBalls;
        extraStartingCabbages = 0;
        holofoilChance = startingHolofoilChance;
        goldenCabbageChance = startingGoldenCabbageChance;
        goldenCabbageValue = startingGoldenCabbageValue;
        shopDiscountMult = 1f;
        shopRarityMult = 1f;
    }
    
    public void AddCoins(double coinsToAdd)
    {
        coins += coinsToAdd;
        CoinsUpdated?.Invoke(coins);
    }
    
    public void AddExtraBall()
    {
        currentBalls++;
    }

    public void AddExtraStartingCabbage()
    {
        extraStartingCabbages++;
    }

    public void AddHolofoilChance(float holofoilChanceAdd)
    {
        holofoilChance += holofoilChanceAdd;
    }

    public void AddGoldenCabbageChance(float goldenChanceAdd)
    {
        goldenCabbageChance += goldenChanceAdd;
    }

    public void AddShopDiscount(float disc)
    {
        shopDiscountMult -= disc;
        if (shopDiscountMult < .5f)
        {
            shopDiscountMult = .5f;
        }
    }

    public void AddShopRarityMult(float rarMultAdd)
    {
        shopRarityMult += rarMultAdd;
    }
}
