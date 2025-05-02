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

    private void OnEnable()
    {
        RunManager.RunFinishedEvent += RunFinishedListener;
    }

    private void OnDisable()
    {
        RunManager.RunFinishedEvent -= RunFinishedListener;
    }

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
    
    
    
    
    // ─── Pet run tracking ───

    /// <summary>
    /// Returns the max difficulty this pet has been beaten at, or 0 if never.
    /// </summary>
    public int GetPetMaxDifficulty(string petID)
    {
        var record = data.petRecords.FirstOrDefault(r => r.petID == petID);
        return record != null ? record.maxDifficulty : 0;
    }

    /// <summary>
    /// Marks that the player has beaten the game with the given pet at this difficulty.
    /// Updates max if higher.
    /// </summary>
    public void RecordPetWin(string petID, int difficulty)
    {
        var record = data.petRecords.FirstOrDefault(r => r.petID == petID);
        if (record == null)
        {
            record = new SaveData.PetRunRecord { petID = petID, maxDifficulty = difficulty };
            data.petRecords.Add(record);
        }
        else if (difficulty > record.maxDifficulty)
        {
            record.maxDifficulty = difficulty;
        }
        SaveToDisk();
    }
    
    public List<(string petID, int maxDifficulty)> GetAllPetRecords()
    {
        return data.petRecords
            .Select(r => (r.petID, r.maxDifficulty))
            .ToList();
    }

    
    
    [Serializable]
    private class SaveData
    {
        public List<string> ownedPetIDs = new List<string>();
        public int metaCurrency = 0;

        // track max difficulty beaten per pet
        public List<PetRunRecord> petRecords = new List<PetRunRecord>();

        // story flags...
        public bool seenOverworldIntro;
        public bool wonFirstRun;
        public bool lostFirstRun;
        public bool hasSeenGameplayTutorial;
        public bool seenShopIntro;
        public bool seenDojoIntro;
        public bool seenLibraryIntro;

        [Serializable]
        public class PetRunRecord
        {
            public string petID;
            public int maxDifficulty;
        }
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

    void RunFinishedListener(RunManager.RunCompleteParams rcp)
    {
        if (!rcp.success)
        {
            return;
        }

        if (rcp.petDefinition != null)
        {
            RecordPetWin(rcp.petDefinition.dataName, rcp.difficulty.difficultyLevel);
        }
    }
}
