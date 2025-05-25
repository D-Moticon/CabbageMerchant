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

        [Tooltip("Weight for random selection.")]
        public float weight = 1f;
    }

    [Tooltip("All potential items and their filters.")]
    public List<ItemInfo> items;

    /// <summary>
    /// Returns a list of items that match the given rarity and any pet requirements.
    /// </summary>
    public List<Item> GetItemsByRarity(Rarity rarity)
    {
        var matchingItems = new List<Item>();
        var equippedPet = Singleton.Instance.petManager.currentPet;

        foreach (var info in items)
        {
            if (!info.enabled) continue;
            if (info.item.requiredPet != null && info.item.requiredPet != equippedPet) continue;
            if (info.item.survivalModeOnly == true && !Singleton.Instance.survivalManager.survivalModeOn) continue;
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
            if (!info.enabled) continue;
            if (info.item.requiredPet != null && info.item.requiredPet != equippedPet) continue;
            matchingItems.Add(info.item);
        }
        return matchingItems;
    }

    /// <summary>
    /// Returns a single random enabled item (uniform distribution).
    /// </summary>
    public Item GetRandomItem()
    {
        var all = GetAllItems();
        if (all == null || all.Count == 0) return null;
        int idx = Random.Range(0, all.Count);
        return all[idx];
    }

    /// <summary>
    /// Returns a single random item, weighted by rarity.
    /// Uses static weights: Common = 1, Rare = 0.5, Legendary = 0.1.
    /// </summary>
    public Item GetRandomItemRarityWeighted()
    {
        var all = GetAllItems();
        if (all == null || all.Count == 0) return null;

        // Static weights per rarity
        const float commonWeight    = 1f;
        const float rareWeight      = 0.1f;
        const float legendaryWeight = 0.05f;

        // Calculate total weight
        float total = 0f;
        foreach (var itm in all)
        {
            switch (itm.rarity)
            {
                case Rarity.Common:    total += commonWeight;    break;
                case Rarity.Rare:      total += rareWeight;      break;
                case Rarity.Legendary: total += legendaryWeight; break;
            }
        }

        // Roll a value in [0, total)
        float r = Random.value * total;
        float acc = 0f;

        // Find the item whose weight range covers r
        foreach (var itm in all)
        {
            switch (itm.rarity)
            {
                case Rarity.Common:    acc += commonWeight;    break;
                case Rarity.Rare:      acc += rareWeight;      break;
                case Rarity.Legendary: acc += legendaryWeight; break;
            }
            if (r <= acc)
                return itm;
        }

        // Safety fallback
        return all[all.Count - 1];
    }
}