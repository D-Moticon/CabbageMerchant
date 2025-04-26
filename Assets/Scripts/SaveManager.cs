using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized save/load of persistent player data: owned pets and metaCurrency.
/// Listens for changes and writes a JSON file in Application.persistentDataPath.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private string saveFilePath;
    private SaveData data;

    public static event System.Action DataLoadedEvent;
    
    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        Load();
    }

    /// <summary>
    /// Load JSON from disk into the internal SaveData. If none exists, initialize defaults.
    /// </summary>
    private void Load()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
        
        else
        {
            data = new SaveData();
        }
        
        DataLoadedEvent?.Invoke();
    }

    /// <summary>
    /// Write the internal SaveData as pretty JSON to disk.
    /// </summary>
    public void SaveToDisk()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
    }

    /// <summary>
    /// Returns a copy of the saved owned pet IDs.
    /// </summary>
    public List<string> GetOwnedPetIDs()
    {
        return new List<string>(data.ownedPetIDs);
    }

    /// <summary>
    /// Overwrites owned pet IDs and saves.
    /// </summary>
    public void SetOwnedPetIDs(List<string> petIDs)
    {
        data.ownedPetIDs = new List<string>(petIDs);
        SaveToDisk();
    }

    /// <summary>
    /// Gets stored metaCurrency value.
    /// </summary>
    public int GetMetaCurrency()
    {
        return data.metaCurrency;
    }

    /// <summary>
    /// Sets metaCurrency and saves.
    /// </summary>
    public void SetMetaCurrency(int amount)
    {
        data.metaCurrency = amount;
        SaveToDisk();
    }

    [Serializable]
    private class SaveData
    {
        public List<string> ownedPetIDs = new List<string>();
        public int metaCurrency = 0;
        
        // ─── NEW STORY FLAGS ───
        public bool seenOverworldIntro = false;
        public bool wonFirstRun        = false;
        public bool lostFirstRun       = false;
        public bool hasSeenGameplayTutorial = false;
        public bool seenShopIntro;
        public bool seenDojoIntro;
        public bool seenLibraryIntro;
    }
    
    /// <summary>
    /// Clears all saved data (pets & metacurrency) and writes defaults to disk.
    /// </summary>
    public void ClearAllData()
    {
        // Reset in-memory data to defaults
        data = new SaveData();
        // Overwrite the file on disk
        SaveToDisk();
        Load();
    }

    
    //STORY GETTERS / SETTERS
    public bool HasSeenOverworldIntro()   => data.seenOverworldIntro;
    public void MarkSeenOverworldIntro()
    {
        data.seenOverworldIntro = true;
        SaveToDisk();
    }

    public bool HasWonFirstRun()           => data.wonFirstRun;
    public void MarkWonFirstRun()
    {
        data.wonFirstRun = true;
        SaveToDisk();
    }

    public bool HasLostFirstRun()          => data.lostFirstRun;
    public void MarkLostFirstRun()
    {
        data.lostFirstRun = true;
        SaveToDisk();
    }

    public bool HasSeenGameplayTutorial() => data.hasSeenGameplayTutorial;

    public void MarkSeenGameplayTutorial()
    {
        data.hasSeenGameplayTutorial = true;
        SaveToDisk();
    }
    
    public bool HasSeenShopIntro() => data.seenShopIntro;

    public void MarkSeenShopIntro()
    {
        data.seenShopIntro = true;
        SaveToDisk();
    }
    
    public bool HasSeenDojoIntro() => data.seenDojoIntro;

    public void MarkSeenDojoIntro()
    {
        data.seenDojoIntro = true;
        SaveToDisk();
    }
    
    public bool HasSeenLibraryIntro() => data.seenLibraryIntro;

    public void MarkSeenLibraryIntro()
    {
        data.seenLibraryIntro = true;
        SaveToDisk();
    }
}
