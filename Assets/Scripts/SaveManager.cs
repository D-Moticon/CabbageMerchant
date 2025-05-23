using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized save/load of persistent player data: skins, chaos cabbages, runs, and story flags.
/// Listens for run completion and writes a JSON file in Application.persistentDataPath.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private string saveFilePath;
    private SaveData data;

    public static event Action DataLoadedEvent;

    private void OnEnable()
    {
        RunManager.RunFinishedEvent += RunFinishedListener;
    }

    private void OnDisable()
    {
        RunManager.RunFinishedEvent -= RunFinishedListener;
    }

    private void Awake()
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

    //=== Owned IDs ===
    public List<string> GetOwnedPetIDs() => new List<string>(data.ownedPetIDs);
    public void SetOwnedPetIDs(List<string> petIDs) { data.ownedPetIDs = new List<string>(petIDs); SaveToDisk(); }

    public List<string> GetOwnedSkinIDs() => new List<string>(data.ownedSkinIDs);
    public void SetOwnedSkinIDs(List<string> skinIDs) { data.ownedSkinIDs = new List<string>(skinIDs); SaveToDisk(); }

    public List<string> GetOwnedChaosCabbageIDs() => new List<string>(data.ownedChaosCabbageIDs);
    public void SetOwnedChaosCabbageIDs(List<string> chaosCabbageIDs) { data.ownedChaosCabbageIDs = new List<string>(chaosCabbageIDs); SaveToDisk(); }

    //=== MetaCurrency ===
    public int GetMetaCurrency() => data.metaCurrency;
    public void SetMetaCurrency(int amount) { data.metaCurrency = amount; SaveToDisk(); }

    //=== Run Records ===
    /// <summary>
    /// Records a completed run, keeping only the fastest time per unique combination, tracking bonk value.
    /// </summary>
    public void RecordRun(string petID, int difficultyLevel, List<string> chaosCabbageIDs, float runTime, double totalBonkValue)
    {
        var sortedChaos = chaosCabbageIDs != null
            ? chaosCabbageIDs.OrderBy(id => id).ToList()
            : new List<string>();

        var existing = data.runRecords.FirstOrDefault(r =>
            r.petID == petID &&
            r.difficultyLevel == difficultyLevel &&
            r.chaosCabbageIDs.Count == sortedChaos.Count &&
            !r.chaosCabbageIDs.Except(sortedChaos).Any()
        );

        if (existing != null)
        {
            if (runTime < existing.runTime)
            {
                existing.runTime = runTime;
                existing.totalBonkValue = totalBonkValue;
            }
        }
        else
        {
            data.runRecords.Add(new SaveData.RunRecord
            {
                petID = petID,
                difficultyLevel = difficultyLevel,
                chaosCabbageIDs = sortedChaos,
                runTime = runTime,
                totalBonkValue = totalBonkValue
            });
        }
        SaveToDisk();
    }

    /// <summary>
    /// Returns all saved run records.
    /// </summary>
    public List<(string petID, int difficultyLevel, List<string> chaosCabbageIDs, float runTime, double totalBonkValue)> GetAllRunRecords()
    {
        return data.runRecords
            .Select(r => (r.petID, r.difficultyLevel, new List<string>(r.chaosCabbageIDs), r.runTime, r.totalBonkValue))
            .ToList();
    }

    //=== Scene run finish listener ===
    private void RunFinishedListener(RunManager.RunCompleteParams rcp)
    {
        if (!rcp.success) return;
        string petID = rcp.petDefinition?.dataName ?? string.Empty;
        int diff = rcp.difficulty?.difficultyLevel ?? 0;
        var chaosIDs = Singleton.Instance.chaosManager.GetEquippedChaosCabbages()
            .Select(c => c.dataName).ToList();
        float time = rcp.runTime;
        double bonkValue = rcp.totalBonkValue;

        RecordRun(petID, diff, chaosIDs, time, bonkValue);
    }
    
    public int GetPetMaxDifficulty(PetDefinition petDef)
    {
        if (petDef == null) return 0;
        var records = data.runRecords.Where(r => r.petID == petDef.dataName);
        return records.Any() ? records.Max(r => r.difficultyLevel) : 0;
    }

    //=== Story Flags ===
    public bool HasSeenOverworldIntro()   => data.seenOverworldIntro;
    public void MarkSeenOverworldIntro() { data.seenOverworldIntro = true; SaveToDisk(); }

    public bool HasWonFirstRun()          => data.wonFirstRun;
    public void MarkWonFirstRun()         { data.wonFirstRun = true; SaveToDisk(); }

    public bool HasLostFirstRun()         => data.lostFirstRun;
    public void MarkLostFirstRun()        { data.lostFirstRun = true; SaveToDisk(); }

    public bool HasSeenGameplayTutorial() => data.hasSeenGameplayTutorial;
    public void MarkSeenGameplayTutorial(){ data.hasSeenGameplayTutorial = true; SaveToDisk(); }

    public bool HasSeenShopIntro()        => data.seenShopIntro;
    public void MarkSeenShopIntro()       { data.seenShopIntro = true; SaveToDisk(); }

    public bool HasSeenDojoIntro()        => data.seenDojoIntro;
    public void MarkSeenDojoIntro()       { data.seenDojoIntro = true; SaveToDisk(); }

    public bool HasSeenLibraryIntro()     => data.seenLibraryIntro;
    public void MarkSeenLibraryIntro()    { data.seenLibraryIntro = true; SaveToDisk(); }

    /// <summary>
    /// Clears all saved data (IDs, runs, meta, story) and writes defaults to disk.
    /// </summary>
    public void ClearAllData()
    {
        data = new SaveData();
        SaveToDisk();
        Load();
    }

    [Serializable]
    private class SaveData
    {
        public List<string> ownedPetIDs = new List<string>();
        public List<string> ownedSkinIDs = new List<string> { "default", "default_M", "defaultDark"};
        public List<string> ownedChaosCabbageIDs = new List<string>();
        public int metaCurrency = 0;

        [Serializable]
        public class RunRecord
        {
            public string petID;
            public int difficultyLevel;
            public List<string> chaosCabbageIDs;
            public float runTime;
            public double totalBonkValue;
        }
        public List<RunRecord> runRecords = new List<RunRecord>();

        // Story flags
        public bool seenOverworldIntro;
        public bool wonFirstRun;
        public bool lostFirstRun;
        public bool hasSeenGameplayTutorial;
        public bool seenShopIntro;
        public bool seenDojoIntro;
        public bool seenLibraryIntro;
    }
}
