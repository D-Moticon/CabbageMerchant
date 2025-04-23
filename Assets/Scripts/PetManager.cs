using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// Manages player-owned pets: overworld spawning, active pet switching, and
/// injecting pets into the run as items on run start.
/// Access via Singleton.Instance.petManager.
/// </summary>
public class PetManager : MonoBehaviour
{
    [Tooltip("Database of all possible pets.")]
    public PetDatabase petDatabase;

    [HideInInspector]
    public PetDefinition currentPet;

    [Tooltip("Overworld pet prefab controller.")]
    public OverworldPet overworldPetPrefab;

    [Tooltip("VFX to spawn when a pet is selected")]
    public PooledObjectData petSelectedVFX;
    [Tooltip("Sound to play when a pet is selected")]
    public SFXInfo petSelectedSFX;

    [Tooltip("Name of the overworld scene where pets should be spawned")]
    public string overworldSceneName = "Overworld";

    [Tooltip("All pets the player has unlocked.")]
    public List<PetDefinition> ownedPets = new List<PetDefinition>();

    // Internal list of live OverworldPet instances
    private List<OverworldPet> overworldPets = new List<OverworldPet>();

    public delegate void PetDefListDelegate(List<PetDefinition> petDefs);
    public static event PetDefListDelegate OwnedPetsChangedEvent;

    void OnEnable()
    {
        RunManager.RunStartEvent += OnRunStart;
    }

    void OnDisable()
    {
        RunManager.RunStartEvent -= OnRunStart;
    }

    private void Start()
    {
        LoadOwnedPetsFromSave();
    }

    public void LoadOwnedPetsFromSave()
    {
        var save = Singleton.Instance.saveManager;
        List<string> savedIds = save.GetOwnedPetIDs();
        ownedPets.Clear();

        foreach (string id in savedIds)
        {
            var def = petDatabase.allPets.Find(p => p.dataName == id);
            if (def != null)
                ownedPets.Add(def);
        }
        // Re-apply the current pet state
        SetCurrentPet(currentPet);
    }

    private void OnRunStart(RunManager.RunStartParams rsp)
    {
        // At the start of each run, inject the current pet into the run items
        if (currentPet != null)
        {
            Singleton.Instance.itemManager.AddPet(currentPet);
        }
    }

    /// <summary>
    /// Called by OverworldPet instances when they enable themselves.
    /// </summary>
    public void RegisterOverworldPet(OverworldPet pet)
    {
        if (!overworldPets.Contains(pet))
            overworldPets.Add(pet);
    }

    /// <summary>
    /// Called by OverworldPet instances when they disable themselves.
    /// </summary>
    public void UnregisterOverworldPet(OverworldPet pet)
    {
        overworldPets.Remove(pet);
    }

    /// <summary>
    /// Sets which pet definition is "active". The matching OverworldPet
    /// will follow; others will wander. Also spawns VFX/SFX and closes the PetShopPanel.
    /// </summary>
    public void SetCurrentPet(PetDefinition def)
    {
        currentPet = def;

        // Update overworld behaviors
        foreach (var pet in overworldPets)
        {
            if (pet.def == def)
                pet.SetFollow();
            else
                pet.SetWander();
        }

        // Spawn VFX and play SFX at the selected pet's position
        var selected = overworldPets.FirstOrDefault(p => p.def == def);
        if (selected != null)
        {
            petSelectedVFX.Spawn(selected.transform.position);
            petSelectedSFX.Play();
        }

        // Close the pet shop panel if open
        Singleton.Instance.menuManager.HideAll();

        // Notify listeners
        OwnedPetsChangedEvent?.Invoke(ownedPets);
    }

    /// <summary>
    /// Spawns all owned pets in the overworld at random positions.
    /// Clears any existing instances first.
    /// </summary>
    public void SpawnOwnedOverworldPets()
    {
        // clean up
        foreach (var pet in new List<OverworldPet>(overworldPets))
        {
            if (pet != null)
                Destroy(pet.gameObject);
        }
        overworldPets.Clear();

        // spawn each
        foreach (var def in ownedPets)
        {
            Vector3 pos = GetRandomSpawnPosition();
            // instantiate into overworld scene
            GameObject go = Instantiate(
                overworldPetPrefab.gameObject,
                pos,
                Quaternion.identity
            );
            // move to overworld scene if available
            var scene = SceneManager.GetSceneByName(overworldSceneName);
            if (scene.IsValid())
                SceneManager.MoveGameObjectToScene(go, scene);

            var pet = go.GetComponent<OverworldPet>();
            pet.def = def;
            if (def == currentPet)
                pet.SetFollow();
            else
                pet.SetWander();
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 rnd = Random.insideUnitCircle * 5f;
        return new Vector3(rnd.x, rnd.y, 0f);
    }

    /// <summary>
    /// Purchases (unlocks) a pet at runtime and spawns it wandering.
    /// </summary>
    public void PurchasePet(PetDefinition def)
    {
        if (!ownedPets.Contains(def))
        {
            ownedPets.Add(def);
            SetCurrentPet(def);
            Vector3 pos = GetRandomSpawnPosition();
            // instantiate into overworld scene
            GameObject go = Instantiate(
                overworldPetPrefab.gameObject,
                pos,
                Quaternion.identity
            );
            var scene = SceneManager.GetSceneByName(overworldSceneName);
            if (scene.IsValid())
                SceneManager.MoveGameObjectToScene(go, scene);

            var pet = go.GetComponent<OverworldPet>();
            pet.def = def;
            pet.SetWander();

            var ids = ownedPets.Select(p => p.dataName).ToList();
            Singleton.Instance.saveManager.SetOwnedPetIDs(ids);

            petSelectedVFX.Spawn(pos);
            petSelectedSFX.Play();
            
            OwnedPetsChangedEvent?.Invoke(ownedPets);
        }
    }

    /// <summary>
    /// Adds a pet definition to ownedPets without saving or spawning.
    /// </summary>
    public void AddPet(PetDefinition def)
    {
        if (!ownedPets.Contains(def))
        {
            ownedPets.Add(def);
        }
    }
}