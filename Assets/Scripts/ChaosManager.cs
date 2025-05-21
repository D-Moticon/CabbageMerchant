using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChaosManager : MonoBehaviour
{
    public ChaosCabbageCollection chaosCabbageCollection;
    public List<ChaosCabbageSO> ownedChaosCabbages;
    private List<ChaosCabbageSO> equippedChaosCabbages = new List<ChaosCabbageSO>();

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

    public void CollectChaosCabbageFromDef(ChaosCabbageSO ccso)
    {
        CollectChaosCabbage(ccso);
    }

    public ChaosCabbageSO GetChaosCabbageFromPetDef(PetDefinition pd)
    {
        for(int i = 0; i < chaosCabbageCollection.chaosCabbages.Count; i++)
        {
            if (chaosCabbageCollection.chaosCabbages[i].petDef == pd)
            {
                return chaosCabbageCollection.chaosCabbages[i];
            }
        }

        return null;
    }

    void CollectChaosCabbage(ChaosCabbageSO ccso)
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
            print(cc.displayName);
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
    
    
}
