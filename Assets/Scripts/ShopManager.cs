using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public ItemSlot itemSlotPrefab;
    public List<Transform> itemSlotTransforms; // Positions for each slot
    public int baseNumberItems = 4;
    public ItemCollection itemCollection;

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
    /// Populates each slot with a random item from the collection, 
    /// respecting rarity weights.
    /// </summary>
    void PopulateItems()
    {
        // Limit the number of items to however many transforms we have
        int numItems = Mathf.Min(baseNumberItems, itemSlotTransforms.Count);

        for (int i = 0; i < numItems; i++)
        {
            ItemSlot slot = createdSlots[i];
            
            // 1) Randomly pick a Rarity using the weighted distribution
            Rarity chosenRarity = GetWeightedRandomRarity();

            // 2) Gather all items of that chosen rarity from itemCollection
            List<Item> validItems = itemCollection.GetItemsByRarity(chosenRarity);
            if (validItems.Count == 0)
            {
                Debug.LogWarning($"No items of rarity {chosenRarity} found in collection.");
                continue;
            }

            // 3) Pick a random item from that subset
            Item itemPrefab = validItems[Random.Range(0, validItems.Count)];

            // 4) Generate and place item into the slot using ItemManager
            Item itemInstance = Singleton.Instance.itemManager.GenerateItemWithWrapper(itemPrefab, Vector2.zero, this.transform);
            itemInstance.purchasable = true;
            Singleton.Instance.itemManager.AddItemToSlot(itemInstance, slot);
            slot.SetPriceText();
        }
    }

    /// <summary>
    /// Removes existing items from slots, then repopulates them with new items.
    /// </summary>
    void ReRoll()
    {
        // 1) Clear out items from each slot
        foreach (var slot in createdSlots)
        {
            if (slot.currentItem)
            {
                // A simple approach might be to destroy the item wrapper
                if (slot.currentItem.itemWrapper)
                {
                    Destroy(slot.currentItem.itemWrapper.gameObject);
                }
                slot.currentItem = null;
            }
        }

        // 2) Populate the newly cleared slots
        PopulateItems();
    }

    /// <summary>
    /// Chooses a rarity from rarityWeights using a weighted random distribution.
    /// </summary>
    /// <returns>A Rarity chosen based on the total weight.</returns>
    private Rarity GetWeightedRandomRarity()
    {
        // Sum the total weight
        float totalWeight = 0f;
        foreach (var rw in rarityWeights)
        {
            totalWeight += rw.weight;
        }

        // Pick a random value within [0..totalWeight]
        float randomVal = Random.value * totalWeight;

        // Determine which rarity bucket we fall into
        foreach (var rw in rarityWeights)
        {
            if (randomVal < rw.weight)
            {
                return rw.rarity;
            }
            randomVal -= rw.weight;
        }

        // Fallback (should never happen if weights are well-defined)
        return rarityWeights[rarityWeights.Count - 1].rarity;
    }

    public void LeaveShop()
    {
        Singleton.Instance.runManager.GoToMap();
    }
}
