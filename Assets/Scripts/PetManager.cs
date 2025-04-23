using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player-owned pets: overworld spawning, active pet switching, and
/// injecting pets into the run as items on run start.
/// Access via Singleton.Instance.petManager.
/// </summary>
public class PetManager : MonoBehaviour
{
    [Tooltip("Currently active pet definition.")]
    public PetDefinition currentPet;

    [Tooltip("Overworld pet prefab controller.")]
    public OverworldPet overworldPetPrefab;

    [Tooltip("All pets the player has unlocked.")]
    public List<PetDefinition> ownedPets = new List<PetDefinition>();

    // Internal list of live OverworldPet instances
    private List<OverworldPet> overworldPets = new List<OverworldPet>();

    void OnEnable()
    {
        RunManager.RunStartEvent += OnRunStart;
    }

    void OnDisable()
    {
        RunManager.RunStartEvent -= OnRunStart;
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
    /// will follow; others will wander.
    /// </summary>
    public void SetCurrentPet(PetDefinition def)
    {
        currentPet = def;
        foreach (var pet in overworldPets)
        {
            if (pet.def == def) pet.SetFollow(); else pet.SetWander();
        }
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
            if (pet != null) Destroy(pet.gameObject);
        }
        overworldPets.Clear();

        // spawn each
        foreach (var def in ownedPets)
        {
            Vector3 pos = GetRandomSpawnPosition();
            var pet = Instantiate(overworldPetPrefab, pos, Quaternion.identity);
            pet.def = def;
            if (def == currentPet) pet.SetFollow(); else pet.SetWander();
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
            var pet = Instantiate(overworldPetPrefab, pos, Quaternion.identity);
            pet.def = def;
            pet.SetWander();
        }
    }
}
