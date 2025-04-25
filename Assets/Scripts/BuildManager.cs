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
        none
    }

    public BuildMode buildMode;

    [Header("TESTING: These are not used for builds")]
    public int startingCoins;
    public bool forceHolofoilStarting = false;
    public PetDefinition startingPet;
    public List<Item> startingItems = new List<Item>();
    public List<Item> startingPerks = new List<Item>();
    public MapBlueprint mapBlueprint;

    public static Action FullGameStartedEvent;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (buildMode)
        {
            case BuildMode.release:
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
                break;
            case BuildMode.startAtGame:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("CabbageShoot");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                break;
            case BuildMode.startAtShop:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.runManager.StartNewRun("Shop");
                Singleton.Instance.menuManager.HideAll();
                PopulateStartingItems();
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                break;
            case BuildMode.none:
                Singleton.Instance.runManager.startingMapBlueprint = mapBlueprint;
                Singleton.Instance.playerStats.AddCoins(startingCoins);
                Singleton.Instance.menuManager.HideAll();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        
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
