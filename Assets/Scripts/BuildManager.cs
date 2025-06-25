using System;
using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public enum BuildMode
    {
        release,
        releaseTest,
        startAtOverworld,
        startRunAtMap,
        startAtGame,
        startAtShop,
        startAtEvent,
        startAtSpecificEvent,
        startAtRestaurant,
        startAtLibrary,
        startAtDojo,
        none
    }

    public string version;
    public BuildMode buildMode;
    
    [Header("DEMO: MAKE SURE TO ADD SCRIPTING DEFINE SYMBOL DEMO IN PLAYER SETTINGS ")]
    public MapBlueprint startingMapBlueprint_DEMO;
    
    [Header("TESTING: These are not used for builds")]
    public int startingCoins;
    public int startingKeys;
    public bool forceHolofoilStarting = false;
    public PetDefinition startingPet;
    public bool unlockAllSlots = false;
    public List<Item> startingItems = new List<Item>();
    public List<Item> startingPerks = new List<Item>();
    public bool randomizeAllPowers = false;
    public bool randomizeAllTriggers = false;
    public Biome startingBiome;
    public MapBlueprint mapBlueprint;
    public Difficulty startingDifficulty;
    public int startingReRolls = 1;
    public Dialogue startingSpecificDialogue;
    public Boss testBoss;
    
    public static Action FullGameStartedEvent;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool isDemo = false;
        
        #if DEMO
                Singleton.Instance.runManager.startingMapBlueprint = startingMapBlueprint_DEMO;
                isDemo = true;
        #endif
        
        switch (buildMode)
        {
            case BuildMode.release:
                if(isDemo)
                {
                    Debug.Log($"Super Cabbage Kabumi Demo Version: {version}");
                }
                else
                {
                    Debug.Log($"Super Cabbage Kabumi Version: {version}");
                }
                
                FullGameStartedEvent?.Invoke();
                break;
            case BuildMode.releaseTest:
                FullGameStartedEvent?.Invoke();
                PopulateStartingItems();
                break;
            case BuildMode.startAtOverworld:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.menuManager.HideAll();
                Singleton.Instance.runManager.GoToSceneExclusive("Overworld");
                break;
            case BuildMode.startRunAtMap:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Map");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                //Need this for survival cabbage to work
                RunManager.RunStartParams rsp = new RunManager.RunStartParams();
                RunManager.RunStartEventLate?.Invoke(rsp);
                break;
            case BuildMode.startAtGame:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("CabbageShoot");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                if (testBoss != null)
                {
                    Singleton.Instance.bossFightManager.SetBossFight(testBoss);
                }
                break;
            case BuildMode.startAtShop:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Shop");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                break;
            case BuildMode.startAtEvent:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Event");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                break;
            case BuildMode.startAtSpecificEvent:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.dialogueManager.nextSpecificDialogue = startingSpecificDialogue;
                Singleton.Instance.runManager.StartNewRun("Event");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                break;
            case BuildMode.startAtRestaurant:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Food");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                break;
            case BuildMode.startAtLibrary:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Library");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                break;
            case BuildMode.startAtDojo:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Dojo");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.runManager.ChangeBiome(startingBiome);
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                Singleton.Instance.petManager.SetCurrentPet(startingPet);
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                Singleton.Instance.playerStats.IncreaseReRolls(startingReRolls);
                break;
            case BuildMode.none:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.playerStats.AddKey(startingKeys);
                Singleton.Instance.menuManager.HideAll();
                Singleton.Instance.playerStats.currentDifficulty = startingDifficulty;
                if (unlockAllSlots)
                {
                    Singleton.Instance.itemManager.UnLockAllInventorySlots();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (buildMode != BuildMode.release && buildMode != BuildMode.releaseTest)
        {
            List<Item> items = Singleton.Instance.itemManager.GetNormalItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (randomizeAllPowers)
                {
                    items[i].RandomizeEffectPowers();
                }

                if (randomizeAllTriggers)
                {
                    items[i].RandomizeTriggers();
                }
            }
        }
    }

    public bool IsDemoMode()
    {
        #if DEMO
                return true;
        #else
                return false;
        #endif
    }
    
    void PopulateStartingItems()
    {
        ItemManager itemManager = Singleton.Instance.itemManager;
        
        if (startingPet != null)
        {
            Singleton.Instance.itemManager.AddPet(startingPet);
        }
        
        for (int i = 0; i < startingItems.Count; i++)
        {
            itemManager.AddItemToInventoryFromPrefab(startingItems[i], forceHolofoilStarting);
        }

        for (int i = 0; i < startingPerks.Count; i++)
        {
            itemManager.AddPerkFromPrefab(startingPerks[i], forceHolofoilStarting);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
