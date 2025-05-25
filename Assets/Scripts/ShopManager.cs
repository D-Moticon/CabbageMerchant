using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    public ItemSlot itemSlotPrefab;
    public List<Transform> itemSlotTransforms;
    public int baseNumberItems = 4;
    public bool onlyBuyOne = false;
    public GameObject onlyBuyOneIndicator;
    public bool noDupes = true;
    [FormerlySerializedAs("lockItems")] public bool lockSlots = false;

    public ItemCollection itemCollection;
    [HideInInspector]public List<Item> spawnedItems = new List<Item>();

    [System.Serializable]
    public class RarityWeight
    {
        public Rarity rarity;
        public float weight = 1f;
    }

    public List<RarityWeight> rarityWeights;

    private List<ItemSlot> createdSlots = new List<ItemSlot>();

    [Header("ReRoll Settings")]
    public int rerollsRemaining;
    public SFXInfo rerollSFX;
    public PooledObjectData rerollVFX;

    public delegate void ShopRerolledDelegate(int rerollsLeft);
    public static event ShopRerolledDelegate ShopRerolledEvent;

    public delegate void ShopDelegate(ShopManager shop);
    public static event ShopDelegate ShopEnteredEvent;

    public delegate void ShopItemDelegate(Item item);
    public static ShopItemDelegate ItemSpawnedInShopEvent;
    
    void Start()
    {
        rerollsRemaining = Singleton.Instance.playerStats.shopReRolls;
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

    private void Awake()
    {
        ShopEnteredEvent?.Invoke(this);
    }

    void CreateItemSlots()
    {
        createdSlots.Clear();
        for (int i = 0; i < itemSlotTransforms.Count; i++)
        {
            ItemSlot newSlot = Instantiate(itemSlotPrefab, itemSlotTransforms[i].position, 
                                           Quaternion.identity, itemSlotTransforms[i]);
            createdSlots.Add(newSlot);
            newSlot.SetPriceText();

            if (lockSlots)
            {
                newSlot.LockSlot();
            }
        }
    }

    void PopulateItems()
    {
        int numItems = Mathf.Min(baseNumberItems, itemSlotTransforms.Count);
        spawnedItems.Clear();
        
        for (int i = 0; i < numItems; i++)
        {
            ItemSlot slot = createdSlots[i];
            Rarity rarityToCheck = GetWeightedRandomRarity();

            List<Item> validItems = itemCollection.GetItemsByRarity(rarityToCheck);
            while (validItems.Count == 0 && rarityToCheck > 0)
            {
                rarityToCheck--;
                validItems = itemCollection.GetItemsByRarity(rarityToCheck);
            }

            if (noDupes)
            {
                validItems = validItems.FindAll(candidate =>
                    spawnedItems.TrueForAll(spawned => spawned.itemName != candidate.itemName));

                while (validItems.Count == 0 && rarityToCheck > 0)
                {
                    rarityToCheck--;
                    validItems = itemCollection.GetItemsByRarity(rarityToCheck);
                }
            }

            if (validItems.Count == 0)
                continue;

            // Select an item based on its weight in the collection
            float totalItemWeight = 0f;
            foreach (var candidate in validItems)
            {
                var info = itemCollection.items.Find(i => i.item == candidate);
                totalItemWeight += (info != null ? info.weight : 1f);
            }

            float randomItemVal = Random.value * totalItemWeight;
            float accumulated = 0f;
            Item itemPrefab = null;
            foreach (var candidate in validItems)
            {
                var info = itemCollection.items.Find(i => i.item == candidate);
                float w = (info != null ? info.weight : 1f);
                accumulated += w;
                if (randomItemVal <= accumulated)
                {
                    itemPrefab = candidate;
                    break;
                }
            }
            if (itemPrefab == null)
                itemPrefab = validItems[validItems.Count - 1];

            Item itemInstance = Singleton.Instance.itemManager.GenerateItemWithWrapper(itemPrefab, Vector2.zero, transform);
            itemInstance.purchasable = true;
            Singleton.Instance.itemManager.AddItemToSlot(itemInstance, slot);
            slot.SetPriceText();
            spawnedItems.Add(itemInstance);
            
            ItemSpawnedInShopEvent?.Invoke(itemInstance);
        }
    }

    public void ReRoll()
    {
        if (rerollsRemaining <= 0)
        {
            return;
        }
        
        if (Singleton.Instance.playerStats.coins < Singleton.Instance.playerStats.reRollCost)
        {
            return;
        }

        rerollsRemaining--;
        rerollSFX?.Play();

        foreach (var slot in createdSlots)
        {
            if (slot.currentItem)
            {
                if (rerollVFX != null)
                    rerollVFX.Spawn(slot.transform.position);

                if (slot.currentItem.itemWrapper)
                    Destroy(slot.currentItem.itemWrapper.gameObject);

                slot.currentItem = null;
            }
        }

        PopulateItems();

        float allHoloRand = Random.Range(0f, 1f);
        if (allHoloRand < Singleton.Instance.playerStats.allHolofoilRollChance)
        {
            foreach (Item item in spawnedItems)
            {
                item.SetHolofoil();
            }
        }
        
        Singleton.Instance.playerStats.AddCoins(-Singleton.Instance.playerStats.reRollCost);
        ShopRerolledEvent?.Invoke(rerollsRemaining);
    }

    private Rarity GetWeightedRandomRarity()
    {
        float totalWeight = 0f;
        float rarityMultiplier = Singleton.Instance.playerStats.shopRarityMult;
        List<float> modifiedWeights = new List<float>();

        foreach (var rw in rarityWeights)
        {
            float modWeight = (rw.rarity != Rarity.Common) ? rw.weight * rarityMultiplier : rw.weight;
            modifiedWeights.Add(modWeight);
            totalWeight += modWeight;
        }

        float randomVal = Random.value * totalWeight;

        for (int i = 0; i < rarityWeights.Count; i++)
        {
            if (randomVal < modifiedWeights[i])
                return rarityWeights[i].rarity;

            randomVal -= modifiedWeights[i];
        }

        return rarityWeights[^1].rarity;
    }

    void ItemPurchasedListener(Item itemPurchased)
    {
        if (onlyBuyOne)
        {
            foreach (var item in spawnedItems)
            {
                if (item != itemPurchased)
                {
                    item.currentItemSlot.HidePriceText();
                    item.DestroyItem(true);
                }
            }
        }
    }

    public void LeaveShop()
    {
        if (Singleton.Instance.pauseManager.isPaused)
        {
            return;
        }
        Singleton.Instance.runManager.GoToMap();
    }

    public void DisableOnlyBuyOne()
    {
        onlyBuyOne = false;
        if (onlyBuyOneIndicator != null)
        {
            onlyBuyOneIndicator.SetActive(false);
        }
    }
}
