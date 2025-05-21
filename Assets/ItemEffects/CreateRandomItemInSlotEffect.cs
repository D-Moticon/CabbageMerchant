using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// Spawns a random item from configured collections in a chosen adjacent slot relative to the owning item.
/// Can optionally spawn upgraded versions and control which direction/slot to use.
/// </summary>
public class CreateRandomItemInSlotEffect : ItemEffect
{
    public enum SpawnDirection
    {
        Above,
        Below,
        Left,
        Right,
        FirstOpen
    }

    [Tooltip("Which adjacent slot to spawn the new item in.")]
    public SpawnDirection spawnDirection = SpawnDirection.Above;

    [Tooltip("Collections of items to choose from.")]
    public List<ItemCollection> itemCollections;

    [Tooltip("How many upgrade tiers above the base item to spawn (0 = base, 1 = upgradedItem, etc).")]
    public int itemLevel = 0;

    [Tooltip("Whether to make the spawned item holofoil.")]
    public bool makeHolofoil = false;

    [Tooltip("Whether to destroy the spawned item when the bounce state exits.")]
    public bool destroyOnBounceExited = true;

    private Item spawnedItem;

    [Tooltip("VFX to play when spawning the item.")]
    public PooledObjectData spawnVFX;

    public string itemDescription;

    private void Awake()
    {
        GameStateMachine.ExitingBounceStateAction += BounceStateExitedListener;
    }

    private void OnDestroy()
    {
        GameStateMachine.ExitingBounceStateAction -= BounceStateExitedListener;
    }

    public override void InitializeItemEffect() { }
    public override void DestroyItemEffect() { }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (owningItem == null || owningItem.currentItemSlot == null)
            return;
        
        var slots = Singleton.Instance.itemManager.itemSlots;
        int currentIndex = slots.IndexOf(owningItem.currentItemSlot);

        if (spawnDirection != SpawnDirection.FirstOpen)
        {
            if (currentIndex < 0)
                return;
        }

        // Determine target slot index
        int targetIndex = -1;
        int columns = Singleton.Instance.itemManager.columns;
        switch (spawnDirection)
        {
            case SpawnDirection.Above:
                targetIndex = currentIndex - columns;
                break;
            case SpawnDirection.Below:
                targetIndex = currentIndex + columns;
                break;
            case SpawnDirection.Left:
                if (currentIndex % columns > 0)
                    targetIndex = currentIndex - 1;
                break;
            case SpawnDirection.Right:
                if (currentIndex % columns < columns - 1)
                    targetIndex = currentIndex + 1;
                break;
            case SpawnDirection.FirstOpen:
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].currentItem == null)
                    {
                        targetIndex = i;
                        break;
                    }
                }
                break;
        }

        // Validate
        if (targetIndex < 0 || targetIndex >= slots.Count)
            return;

        var targetSlot = slots[targetIndex];
        if (targetSlot.currentItem != null)
            return;

        // Gather all candidates
        var allCandidates = new List<Item>();
        foreach (var col in itemCollections)
        {
            var items = col.GetAllItems();
            if (items != null)
                allCandidates.AddRange(items);
        }
        if (allCandidates.Count == 0)
            return;

        // Pick a random base item
        Item baseItem = allCandidates[Random.Range(0, allCandidates.Count)];

        // Climb upgrade chain
        Item selectedItem = baseItem;
        for (int lvl = 0; lvl < itemLevel; lvl++)
        {
            if (selectedItem.upgradedItem != null)
                selectedItem = selectedItem.upgradedItem;
            else
                break;
        }

        // Spawn and configure
        Vector3 spawnPos = targetSlot.transform.position;
        Item newItem = Singleton.Instance.itemManager.GenerateItemWithWrapper(selectedItem, spawnPos);
        if (makeHolofoil)
            newItem.SetHolofoil();
        newItem.SetNormalizedPrice(0f);
        Singleton.Instance.itemManager.AddItemToSlot(newItem, targetSlot);

        spawnedItem = newItem;
        if (spawnVFX != null)
            spawnVFX.Spawn(newItem.transform.position);
    }

    public override string GetDescription()
    {
        string desc = string.IsNullOrEmpty(itemDescription) ? "item" : itemDescription;
        string destroyStr = destroyOnBounceExited ? " Destroys when bounce ends." : string.Empty;
        return $"Spawn a random Level {itemLevel + 1} {desc} in the {spawnDirection} slot if empty.{destroyStr}";
    }

    private void BounceStateExitedListener()
    {
        if (destroyOnBounceExited && spawnedItem != null)
        {
            spawnedItem.DestroyItem(false, false);
            spawnedItem = null;
        }
    }
}
