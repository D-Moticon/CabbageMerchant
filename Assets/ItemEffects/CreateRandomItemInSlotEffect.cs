using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns a random item from configured collections in the slot above the owning item, if that slot is empty.
/// Can optionally spawn the item's upgraded versions based on itemLevel.
/// </summary>
public class CreateRandomItemInSlotEffect : ItemEffect
{
    [Tooltip("Collections of items to choose from.")]
    public List<ItemCollection> itemCollections;

    [Tooltip("How many upgrade tiers above the base item to spawn (0 = base, 1 = upgradedItem, etc).")]
    public int itemLevel = 0;

    [Tooltip("Whether to make the spawned item holofoil.")]
    public bool makeHolofoil = false;
    public bool destroyOnBounceExited = true;

    private Item spawnedItem;

    [Tooltip("VFX to play when spawning the item.")]
    public PooledObjectData spawnVFX;

    public string itemDescription;

    public override void InitializeItemEffect()
    {
        GameStateMachine.ExitingBounceStateAction += BounceStateExitedListener;
    }

    public override void DestroyItemEffect()
    {
        GameStateMachine.ExitingBounceStateAction -= BounceStateExitedListener;
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (owningItem == null || owningItem.currentItemSlot == null)
            return;

        var slots = Singleton.Instance.itemManager.itemSlots;
        int currentIndex = slots.IndexOf(owningItem.currentItemSlot);
        if (currentIndex < 0)
            return;

        const int columns = 2;
        int aboveIndex = currentIndex - columns;
        if (aboveIndex < 0)
            return;

        var aboveSlot = slots[aboveIndex];
        if (aboveSlot.currentItem != null)
            return;

        if (itemCollections == null || itemCollections.Count == 0)
            return;

        // Flatten all collections into one candidate list
        var allCandidates = new List<Item>();
        foreach (var col in itemCollections)
        {
            var items = col.GetAllItems();
            if (items != null && items.Count > 0)
                allCandidates.AddRange(items);
        }
        if (allCandidates.Count == 0)
            return;

        // Pick a random base item
        int randomIndex = Random.Range(0, allCandidates.Count);
        Item baseItem = allCandidates[randomIndex];

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
        Vector3 spawnPos = aboveSlot.transform.position;
        Item newItem = Singleton.Instance.itemManager.GenerateItemWithWrapper(selectedItem, spawnPos);
        if (makeHolofoil)
            newItem.SetHolofoil();
        newItem.SetNormalizedPrice(0f);
        Singleton.Instance.itemManager.AddItemToSlot(newItem, aboveSlot);

        spawnedItem = newItem;
        if (spawnVFX != null)
            spawnVFX.Spawn(spawnedItem.transform.position);
    }

    public override string GetDescription()
    {
        string itemDesc = "item";
        if (!string.IsNullOrEmpty(itemDescription))
        {
            itemDesc = itemDescription;
        }

        string destroyString = "";
        
        if (destroyOnBounceExited)
        {
            destroyString = "Destroy when shot ended.";
        }
        return $"Spawn a random Level {itemLevel + 1} {itemDesc} in the slot above this one if that slot is empty. {destroyString}";
    }

    private void BounceStateExitedListener()
    {
        if (destroyOnBounceExited)
        {
            if (spawnedItem != null)
                spawnedItem.DestroyItem(false, false);
        }
    }
}
