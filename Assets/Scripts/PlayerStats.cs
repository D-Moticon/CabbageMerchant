using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStats : MonoBehaviour
{
    public double startingCoins = 0;

    [HideInInspector]public RunStats currentRunStats;
    
    public int startingBalls = 3;
    [FormerlySerializedAs("currentBalls")] [HideInInspector]public int maxBalls = 3;
    [HideInInspector] public int extraStartingCabbages = 0;
    
    [HideInInspector]public double coins;
    [HideInInspector] public double metaCurrency;
    public float startingHolofoilChance = 0.005f;
    [HideInInspector] public float holofoilChance = 0.005f;
    public float startingGoldenCabbageChance = 0.01f;
    [HideInInspector] public float goldenCabbageChance = 0.01f;
    public float startingGoldenCabbageValue = 3f;
    [HideInInspector] public float goldenCabbageValue = 3f;
    [HideInInspector] public float shopDiscountMult = 1f;
    [HideInInspector] public float shopRarityMult = 1f;
    [HideInInspector] public int keyValue = 1;
    public int startingShopReRolls = 1;
    [HideInInspector] public int shopReRolls = 1;
    public double startingReRollCost = 5;
    [HideInInspector]public double reRollCost = 5;
    [HideInInspector] public float allHolofoilRollChance = 0f;
    [HideInInspector] public float weaponCooldownSpeedMult = 1f;
    public int startingLives = 1;
    [HideInInspector] public int lives = 1;
    public int startingMaxCoins = 99;
    [HideInInspector] public int maxCoins = 99;
    [HideInInspector] public int numberKeys = 0;
    public float startingKeyChance = 0.25f;
    [HideInInspector]public float keyChance = 0.34f;
    [HideInInspector] public int currentMapLayer = 0;
    public Difficulty currentDifficulty;
    [HideInInspector] public float timerAdd = 0f;

    [HideInInspector] public double totalCabbagesBonkedThisRun;
    [HideInInspector] public double totalBonkValueThisRun;
    [HideInInspector] public float totalRunTime = 0f;
    [HideInInspector] public int totalConsumablesUsedThisRun = 0;
    
    public delegate void DoubleEvent(double value);
    public static DoubleEvent CoinsUpdated;

    public delegate void IntEvent(int value);
    public static event IntEvent LivesUpdated;
    public static Action LifeLostEvent;
    public static Action LifeGainedEvent;
    public static event IntEvent KeysUpdatedEvent;
    public static event DoubleEvent MetacurrencyUpdatedEvent;
    public static IntEvent ConsumablesUsedUpdatedEvent;
    
    private void OnEnable()
    {
        RunManager.RunStartEvent += StartRunListener;
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
        MapGenerator.MapGeneratedEvent += MapGeneratedListener;
        SaveManager.DataLoadedEvent += SaveDataLoadedListener;
        ItemManager.ItemSoldEvent += ItemSoldListener;
        GetMetacurrencyFromSave();
    }

    private void OnDisable()
    {
        RunManager.RunStartEvent -= StartRunListener;
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
        MapGenerator.MapGeneratedEvent -= MapGeneratedListener;
        SaveManager.DataLoadedEvent -= SaveDataLoadedListener;
        ItemManager.ItemSoldEvent -= ItemSoldListener;
    }

    void GetMetacurrencyFromSave()
    {
        metaCurrency = Singleton.Instance.saveManager.GetMetaCurrency();
        MetacurrencyUpdatedEvent?.Invoke(metaCurrency);
    }
    
    void StartRunListener(RunManager.RunStartParams rsp)
    {
        coins = startingCoins;
        CoinsUpdated?.Invoke(coins);
        
        maxBalls = startingBalls;
        extraStartingCabbages = 0;
        holofoilChance = startingHolofoilChance;
        goldenCabbageChance = startingGoldenCabbageChance;
        goldenCabbageValue = startingGoldenCabbageValue;
        keyValue = 1;
        shopDiscountMult = 1f;
        shopRarityMult = 1f;
        shopReRolls = startingShopReRolls;
        reRollCost = startingReRollCost;
        allHolofoilRollChance = 0f;
        weaponCooldownSpeedMult = 1f;
        lives = startingLives;
        LivesUpdated?.Invoke(lives);
        maxCoins = startingMaxCoins;
        numberKeys = 0;
        KeysUpdatedEvent?.Invoke(numberKeys);
        keyChance = startingKeyChance;
        currentMapLayer = 0;
        totalCabbagesBonkedThisRun = 0;
        totalBonkValueThisRun = 0;
        totalRunTime = 0;
        totalConsumablesUsedThisRun = 0;
        timerAdd = 0f;
    }

    private void Update()
    {
        totalRunTime += Time.deltaTime;
    }

    public void AddCoins(double coinsToAdd)
    {
        coins += coinsToAdd;
        coins = Math.Round(coins);
        
        if (coins > maxCoins)
        {
            coins = maxCoins;
        }

        if (coins < 0)
        {
            coins = 0;
        }
        
        CoinsUpdated?.Invoke(coins);
    }
    
    public void IncreaseMaxBalls()
    {
        maxBalls++;
        if (maxBalls > 5)
        {
            maxBalls = 5;
        }
        
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

    public void ReduceReRollCost(double costReduction)
    {
        reRollCost -= costReduction;
        if (reRollCost < 1)
        {
            reRollCost = 1;
        }
    }

    public void IncreaseReRolls(int number)
    {
        shopReRolls += number;
    }

    public void AddAllHolofoilRollChance(float chanceAdd)
    {
        allHolofoilRollChance += chanceAdd;
    }

    public void AddWeaponCooldownSpeedMult(float multAdd)
    {
        weaponCooldownSpeedMult += multAdd;
    }

    public void AddLife(int livesToAdd)
    {
        lives += livesToAdd;
        if (lives > 10)
        {
            lives = 10;
        }
        LivesUpdated?.Invoke(lives);
        LifeGainedEvent?.Invoke();
    }

    public void RemoveLife()
    {
        lives--;
        LivesUpdated?.Invoke(lives);
        LifeLostEvent?.Invoke();
    }

    public void AddMaxCoins(int amount)
    {
        maxCoins += amount;
    }

    public void AddKey(int amount)
    {
        numberKeys += amount*keyValue;
        KeysUpdatedEvent?.Invoke(numberKeys);
    }

    public void RemoveKey(int amount)
    {
        numberKeys -= amount;
        KeysUpdatedEvent?.Invoke(numberKeys);
    }

    public void AddMetacurrency(int amount)
    {
        metaCurrency += amount;
        if (metaCurrency < 0) metaCurrency = 0;
        Singleton.Instance.saveManager.SetMetaCurrency((int)metaCurrency);
        MetacurrencyUpdatedEvent?.Invoke(metaCurrency);
    }

    void CabbageBonkedListener(BonkParams bonkParams)
    {
        totalCabbagesBonkedThisRun += bonkParams.bonkerPower;
        totalBonkValueThisRun += bonkParams.totalBonkValueGained;
    }

    void MapGeneratedListener(Map map, MapBlueprint blueprint)
    {
        
    }

    void SaveDataLoadedListener()
    {
        GetMetacurrencyFromSave();
    }

    void ItemSoldListener(Item item)
    {
        if (item.itemType == Item.ItemType.Consumable)
        {
            totalConsumablesUsedThisRun++;
            ConsumablesUsedUpdatedEvent?.Invoke(totalConsumablesUsedThisRun);
        }
    }

    public void AddTimerDuration(float durAdd)
    {
        timerAdd += durAdd;
    }
}
