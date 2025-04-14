using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    public ItemSlot itemSlotPrefab;
    public List<Transform> itemSlotTransforms; // Positions for each slot
    public int baseNumberItems = 4;
    public bool onlyBuyOne = false;
    
    [Tooltip("Prevents duplicate items from spawning in the shop if set to true.")]
    public bool noDupes = true;

    public ItemCollection itemCollection;
    private List<Item> spawnedItems = new List<Item>();

    [System.Serializable]
    public class RarityWeight
    {
        public Rarity rarity;
        public float weight = 1f;
    }

    public List<RarityWeight> rarityWeights;

    // Optional: Track created slots so ReRoll can easily destroy them
    private List<ItemSlot> createdSlots = new List<ItemSlot>();

    void Start()
    {
        CreateItemSlots();
        PopulateItems();
    }

    private void OnEnable()
    {
        ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
    }

    private void OnDisable()
    {
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
    }

    /// <summary>
    /// Creates a new ItemSlot at each Transform in itemSlotTransforms.
    /// </summary>
    void CreateItemSlots()
    {
        // First clear any old references
        createdSlots.Clear();

        for (int i = 0; i < itemSlotTransforms.Count; i++)
        {
            // Create item slots as child objects
            ItemSlot newSlot = Instantiate(itemSlotPrefab, itemSlotTransforms[i].position, 
                                           Quaternion.identity, itemSlotTransforms[i]);
            createdSlots.Add(newSlot);
            newSlot.SetPriceText(); //to hide the price tag
        }
    }

    /// <summary>
    /// Populates each slot with a random item from the collection, respecting rarity weights.
    /// If noDupes is true, duplicate items (by name) will not be spawned.
    /// </summary>
    void PopulateItems()
{
    // Limit the number of items to however many transforms we have
    int numItems = Mathf.Min(baseNumberItems, itemSlotTransforms.Count);
    spawnedItems.Clear();
    
    for (int i = 0; i < numItems; i++)
    {
        ItemSlot slot = createdSlots[i];
        
        // 1) Randomly pick a Rarity using the weighted distribution
        Rarity chosenRarity = GetWeightedRandomRarity();

        // 2) Gather all items of that chosen rarity from itemCollection.
        // If none exist, try lower rarities until we find some or exhaust the enum.
        List<Item> validItems = itemCollection.GetItemsByRarity(chosenRarity);
        Rarity rarityToCheck = chosenRarity;
        while (validItems.Count == 0)
        {
            int nextLower = (int)rarityToCheck - 1;
            if (nextLower < 0)
            {
                print("breaking");
                break;
            }
            rarityToCheck = (Rarity)nextLower;
            validItems = itemCollection.GetItemsByRarity(rarityToCheck);
        }

        if (validItems.Count == 0)
        {
            Debug.LogWarning($"No items of rarity {chosenRarity} or any lower rarity found in collection.");
            continue;
        }
        
        // 3) If noDupes is enabled, filter out any item with an itemName that already appears in spawnedItems.
        if (noDupes)
        {
            validItems = validItems.FindAll(candidate =>
                spawnedItems.TrueForAll(spawned => spawned.itemName != candidate.itemName)
            );

            while (validItems.Count == 0)
            {
                int nextLower = (int)rarityToCheck - 1;
                if (nextLower < 0)
                {
                    print("breaking");
                    break;
                }
                rarityToCheck = (Rarity)nextLower;
                validItems = itemCollection.GetItemsByRarity(rarityToCheck);
            }

            if (validItems.Count == 0)
            {
                Debug.LogWarning($"No items of rarity {chosenRarity} or any lower rarity found in collection.");
                continue;
            }
        }

        // 4) Pick a random item from the (filtered) subset
        Item itemPrefab = validItems[Random.Range(0, validItems.Count)];

        // 5) Generate and place the item into the slot.
        Item itemInstance = Singleton.Instance.itemManager.GenerateItemWithWrapper(itemPrefab, Vector2.zero, this.transform);
        itemInstance.purchasable = true;
        Singleton.Instance.itemManager.AddItemToSlot(itemInstance, slot);
        slot.SetPriceText();
        spawnedItems.Add(itemInstance);
    }
}


    /// <summary>
    /// Removes existing items from slots and repopulates them with new items.
    /// </summary>
    void ReRoll()
    {
        // 1) Clear out items from each slot
        foreach (var slot in createdSlots)
        {
            if (slot.currentItem)
            {
                // Destroy the item wrapper (and thus the item)
                if (slot.currentItem.itemWrapper)
                {
                    Destroy(slot.currentItem.itemWrapper.gameObject);
                }
                slot.currentItem = null;
            }
        }

        // 2) Repopulate the cleared slots
        PopulateItems();
    }

    /// <summary>
    /// Chooses a rarity from rarityWeights using a weighted random distribution.
    /// </summary>
    /// <returns>A Rarity chosen based on the total weight.</returns>
    private Rarity GetWeightedRandomRarity()
    {
        float totalWeight = 0f;
        float rarityMultiplier = Singleton.Instance.playerStats.shopRarityMult;
        // Create a parallel list for the modified weights.
        List<float> modifiedWeights = new List<float>();

        foreach (var rw in rarityWeights)
        {
            float modWeight = rw.weight;
            // If the rarity is not Common, multiply its weight by the rarity multiplier.
            // (Adjust this logic if you want to affect only certain rarities.)
            if (rw.rarity != Rarity.Common)
            {
                modWeight *= rarityMultiplier;
            }

            modifiedWeights.Add(modWeight);
            totalWeight += modWeight;
        }

        // Pick a random value within [0..totalWeight]
        float randomVal = Random.value * totalWeight;

        // Determine which rarity bucket we fall into.
        for (int i = 0; i < rarityWeights.Count; i++)
        {
            if (randomVal < modifiedWeights[i])
            {
                return rarityWeights[i].rarity;
            }
            randomVal -= modifiedWeights[i];
        }

        // Fallback (should never happen if weights are well-defined)
        return rarityWeights[rarityWeights.Count - 1].rarity;
    }


    public void LeaveShop()
    {
        Singleton.Instance.runManager.GoToMap();
    }

    void ItemPurchasedListener(Item itemPurchased)
    {
        if (onlyBuyOne)
        {
            for (int i = 0; i < spawnedItems.Count; i++)
            {
                if (spawnedItems[i] != itemPurchased)
                {
                    spawnedItems[i].currentItemSlot.HidePriceText();
                    spawnedItems[i].DestroyItem(true);
                }
            }
        }
    }
}
