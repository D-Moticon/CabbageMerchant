// ItemCollection.cs
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

        [Tooltip("If true, this item appears in both demo and full builds; if false, full-game only.")]
        public bool InDemo = false;
    }

    [Tooltip("All potential items and their filters.")]
    public List<ItemInfo> items;

    /// <summary>
    /// Returns a list of items that match the given rarity and any pet/survival requirements,
    /// and excludes full-only items in demo mode.
    /// </summary>
    public List<Item> GetItemsByRarity(Rarity rarity)
    {
        var matchingItems = new List<Item>();
        bool demo = Singleton.Instance.buildManager.IsDemoMode();
        var equippedPet = Singleton.Instance.petManager.currentPet;

        foreach (var info in items)
        {
            if (!info.enabled) 
                continue;

            // demo builds only get InDemo==true items
            if (demo && !info.InDemo) 
                continue;

            if (info.item.requiredPet != null && info.item.requiredPet != equippedPet) 
                continue;

            if (info.item.survivalModeOnly && !Singleton.Instance.survivalManager.survivalModeOn) 
                continue;

            if (info.item.rarity == rarity)
                matchingItems.Add(info.item);
        }
        return matchingItems;
    }

    /// <summary>
    /// Returns all items that meet the enabled and pet requirements,
    /// and excludes full-only items in demo mode.
    /// </summary>
    public List<Item> GetAllItems()
    {
        var matchingItems = new List<Item>();
        bool demo = Singleton.Instance.buildManager.IsDemoMode();
        var equippedPet = Singleton.Instance.petManager.currentPet;

        foreach (var info in items)
        {
            if (!info.enabled) 
                continue;

            if (demo && !info.InDemo) 
                continue;

            if (info.item.requiredPet != null && info.item.requiredPet != equippedPet) 
                continue;

            matchingItems.Add(info.item);
        }
        return matchingItems;
    }

    /// <summary>
    /// Returns a single random enabled item (uniform distribution),
    /// respecting demo/full rules.
    /// </summary>
    public Item GetRandomItem()
    {
        var all = GetAllItems();
        if (all == null || all.Count == 0) return null;
        int idx = Random.Range(0, all.Count);
        return all[idx];
    }

    /// <summary>
    /// Returns a single random item, weighted by rarity,
    /// respecting demo/full rules.
    /// </summary>
    public Item GetRandomItemRarityWeighted()
    {
        var all = GetAllItems();
        if (all == null || all.Count == 0) return null;

        const float commonWeight    = 1f;
        const float rareWeight      = 0.1f;
        const float legendaryWeight = 0.05f;

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

        float r = Random.value * total;
        float acc = 0f;

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

        return all[all.Count - 1];
    }
}
