using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemCollection", menuName = "Scriptable Objects/ItemCollection")]
public class ItemCollection : ScriptableObject
{
    [System.Serializable]
    public class ItemInfo
    {
        [Tooltip("The item prefab to include.")]
        public Item item;

        [Tooltip("Enable or disable this entry.")]
        public bool enabled = true;

        [Tooltip("Optional: only show this item when the specified pet is equipped.")]
        public PetDefinition requiredPet;
    }

    [Tooltip("All potential items and their filters.")]
    public List<ItemInfo> items;

    /// <summary>
    /// Returns a list of items that match the given rarity and any pet requirements.
    /// </summary>
    public List<Item> GetItemsByRarity(Rarity rarity)
    {
        var matchingItems = new List<Item>();
        // Get the currently equipped pet, or null if none
        var equippedPet = Singleton.Instance.petManager.currentPet;

        foreach (var info in items)
        {
            if (!info.enabled)
                continue;
            // Skip if a pet is required but not equipped
            if (info.requiredPet != null && info.requiredPet != equippedPet)
                continue;
            if (info.item.rarity == rarity)
                matchingItems.Add(info.item);
        }
        return matchingItems;
    }

    /// <summary>
    /// Returns all items that meet the enabled and pet requirements.
    /// </summary>
    public List<Item> GetAllItems()
    {
        var matchingItems = new List<Item>();
        var equippedPet = Singleton.Instance.petManager.currentPet;

        foreach (var info in items)
        {
            if (!info.enabled)
                continue;
            if (info.requiredPet != null && info.requiredPet != equippedPet)
                continue;
            matchingItems.Add(info.item);
        }
        return matchingItems;
    }
}