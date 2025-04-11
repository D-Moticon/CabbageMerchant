using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public double startingCoins = 0;
    [HideInInspector]public double coins;

    public delegate void DoubleEvent(double value);

    public static DoubleEvent CoinsUpdated;

    private void Start()
    {
        coins = startingCoins;
        CoinsUpdated?.Invoke(coins);
    }

    public void AddCoins(double coinsToAdd)
    {
        coins += coinsToAdd;
        CoinsUpdated?.Invoke(coins);
    }
}
