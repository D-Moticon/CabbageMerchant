using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class ChaosManager : MonoBehaviour
{
    public ChaosCabbageCollection chaosCabbageCollection;
    public List<ChaosCabbageSO> ownedChaosCabbages;
    public List<ChaosCabbageSO> equippedChaosCabbages = new List<ChaosCabbageSO>();

    public Dialogue chaosCabbageCollectedInstructions;
    
    public class ChaosCabbageGetParams
    {
        public ChaosCabbageSO ccso;
    }

    public delegate void ChaosCabbageGetDelegate(ChaosCabbageGetParams ccgp);
    public static ChaosCabbageGetDelegate ChaosCabbageGetEvent;

    void OnEnable()
    {
        RunManager.RunStartEvent += OnRunStart;
        SaveManager.DataLoadedEvent += SaveDataLoaded;
    }

    void OnDisable()
    {
        RunManager.RunStartEvent -= OnRunStart;
        SaveManager.DataLoadedEvent -= SaveDataLoaded;
    }
    
    private void Start()
    {
        LoadOwnedCCsFromSave();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(CollectChaosCabbageTask(chaosCabbageCollection.chaosCabbages[0]));
        }
    }

    public ChaosCabbageSO GetChaosCabbageFromPetDef(PetDefinition pd)
    {
        if (pd == null)
        {
            for(int i = 0; i < chaosCabbageCollection.chaosCabbages.Count; i++)
            {
                if (chaosCabbageCollection.chaosCabbages[i].petDef == null)
                {
                    return chaosCabbageCollection.chaosCabbages[i];
                }
            } 
        }
        
        for(int i = 0; i < chaosCabbageCollection.chaosCabbages.Count; i++)
        {
            if (chaosCabbageCollection.chaosCabbages[i].petDef == pd)
            {
                return chaosCabbageCollection.chaosCabbages[i];
            }
        }

        return null;
    }

    public IEnumerator CollectChaosCabbageTask(ChaosCabbageSO ccso)
    {
        AddChaosCabbageToSaveFile(ccso);

        Dialogue d = ccso.cabbageGetDialogue;

        yield return new WaitForSeconds(3f);
        
        Task dialogueTask = new Task(Singleton.Instance.dialogueManager.DialogueTaskRoutine(d));
        while (dialogueTask.Running)
        {
            yield return null;
        }
        
        Task instructionsTask = new Task(Singleton.Instance.dialogueManager.DialogueTaskRoutine(chaosCabbageCollectedInstructions));
        while (instructionsTask.Running)
        {
            yield return null;
        }
    }

    public void AddChaosCabbageToSaveFile(ChaosCabbageSO ccso)
    {
        ChaosCabbageGetParams ccgp = new ChaosCabbageGetParams();
        ccgp.ccso = ccso;
        ChaosCabbageGetEvent?.Invoke(ccgp);

        if (!ownedChaosCabbages.Contains(ccso))
        {
            ownedChaosCabbages.Add(ccso);
        }
        
        var ids = ownedChaosCabbages.Select(c => c.dataName).ToList();
        Singleton.Instance.saveManager.SetOwnedChaosCabbageIDs(ids);
    }
    
    public void LoadOwnedCCsFromSave()
    {
        var save = Singleton.Instance.saveManager;
        List<string> savedIds = save.GetOwnedChaosCabbageIDs();
        ownedChaosCabbages.Clear();

        foreach (string id in savedIds)
        {
            var def = chaosCabbageCollection.chaosCabbages.Find(c => c.dataName == id);
            if (def != null)
                ownedChaosCabbages.Add(def);
        }
    }

    void SaveDataLoaded()
    {
        LoadOwnedCCsFromSave();
    }
    
    private void OnRunStart(RunManager.RunStartParams rsp)
    {
        foreach (var cc in equippedChaosCabbages)
        {
            Singleton.Instance.itemManager.AddPerkFromPrefab(cc.item);
        }
        
    }

    public bool IsChaosCabbageUnlocked(ChaosCabbageSO ccso)
    {
        if (ownedChaosCabbages.Contains(ccso))
        {
            return true;
        }

        return false;
    }

    public bool IsChaosCabbageEquipped(ChaosCabbageSO ccso)
    {
        if (equippedChaosCabbages.Contains(ccso))
        {
            return true;
        }

        return false;
    }

    public void EquipChaosCabbage(ChaosCabbageSO ccso)
    {
        if (equippedChaosCabbages.Contains(ccso))
        {
            return;
        }
        
        equippedChaosCabbages.Add(ccso);
    }

    public void UnequipChaosCabbage(ChaosCabbageSO ccso)
    {
        if (equippedChaosCabbages.Contains(ccso))
        {
            equippedChaosCabbages.Remove(ccso);
        }
    }

    public List<ChaosCabbageSO> GetEquippedChaosCabbages()
    {
        return equippedChaosCabbages;
    }
}
