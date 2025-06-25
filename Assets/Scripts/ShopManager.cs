// ShopManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    public ItemSlot itemSlotPrefab;
    public List<Transform> itemSlotTransforms;
    public int baseNumberItems = 4;
    public bool onlyBuyOne = false;
    public GameObject onlyBuyOneIndicator;
    public bool noDupes = true;
    public bool lockSlots = false;
    public float rerollCostMultiplier = 1f;

    public ItemCollection itemCollection;
    [HideInInspector] public List<Item> spawnedItems = new List<Item>();

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
        PlayerStats.ReRollsIncreasedEvent += ReRollsIncreasedListener;
    }

    private void OnDisable()
    {
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
        PlayerStats.ReRollsIncreasedEvent -= ReRollsIncreasedListener;
    }

    private void Awake()
    {
        ShopEnteredEvent?.Invoke(this);
    }

    void CreateItemSlots()
    {
        createdSlots.Clear();
        foreach (var t in itemSlotTransforms)
        {
            var slot = Instantiate(itemSlotPrefab, t.position, Quaternion.identity, t);
            slot.SetPriceText();
            if (lockSlots) slot.LockSlot();
            createdSlots.Add(slot);
        }
    }

    void PopulateItems()
    {
        int numItems = Mathf.Min(baseNumberItems, itemSlotTransforms.Count);
        spawnedItems.Clear();

        for (int i = 0; i < numItems; i++)
        {
            var slot = createdSlots[i];
            var rarityToCheck = GetWeightedRandomRarity();

            // fetch only enabled + demo-allowed + pet + survival filtered items:
            var validItems = itemCollection.GetItemsByRarity(rarityToCheck);

            // fallback rarity down if none at this tier
            while (validItems.Count == 0 && rarityToCheck > 0)
            {
                rarityToCheck--;
                validItems = itemCollection.GetItemsByRarity(rarityToCheck);
            }

            // remove duplicates if needed
            if (noDupes)
            {
                validItems = validItems.FindAll(cand =>
                    spawnedItems.TrueForAll(spawned => spawned.itemName != cand.itemName));

                while (validItems.Count == 0 && rarityToCheck > 0)
                {
                    rarityToCheck--;
                    validItems = itemCollection.GetItemsByRarity(rarityToCheck);
                }
            }

            if (validItems.Count == 0) 
                continue;

            // pick one by weight
            float totalWeight = 0f;
            foreach (var cand in validItems)
            {
                var info = itemCollection.items.Find(ii => ii.item == cand);
                totalWeight += (info != null ? info.weight : 1f);
            }

            float pick = Random.value * totalWeight;
            float acc = 0f;
            Item chosen = null;
            foreach (var cand in validItems)
            {
                var info = itemCollection.items.Find(ii => ii.item == cand);
                float w = (info != null ? info.weight : 1f);
                acc += w;
                if (pick <= acc)
                {
                    chosen = cand;
                    break;
                }
            }
            if (chosen == null)
                chosen = validItems[validItems.Count - 1];

            // spawn it
            var inst = Singleton.Instance.itemManager
                        .GenerateItemWithWrapper(chosen, Vector2.zero, transform);
            inst.purchasable = true;
            Singleton.Instance.itemManager.AddItemToSlot(inst, slot);
            slot.SetPriceText();
            spawnedItems.Add(inst);

            ItemSpawnedInShopEvent?.Invoke(inst);
        }
    }

    public void ReRoll()
    {
        double cost = Singleton.Instance.playerStats.reRollCost * rerollCostMultiplier;
        
        if (rerollsRemaining <= 0 || Singleton.Instance.playerStats.coins < cost)
            return;

        rerollsRemaining--;
        rerollSFX?.Play();

        foreach (var slot in createdSlots)
        {
            if (slot.currentItem)
            {
                rerollVFX?.Spawn(slot.transform.position);
                if (slot.currentItem.itemWrapper)
                    Destroy(slot.currentItem.itemWrapper.gameObject);
                slot.currentItem = null;
            }
        }

        PopulateItems();
        if (Random.value < Singleton.Instance.playerStats.allHolofoilRollChance)
        {
            foreach (var it in spawnedItems) it.SetHolofoil();
        }

        Singleton.Instance.playerStats.AddCoins(-cost);
        ShopRerolledEvent?.Invoke(rerollsRemaining);
    }

    private Rarity GetWeightedRandomRarity()
    {
        float total = 0f;
        float mult = Singleton.Instance.playerStats.shopRarityMult;
        var mods = new List<float>();

        foreach (var rw in rarityWeights)
        {
            float w = (rw.rarity != Rarity.Common) 
                      ? rw.weight * mult 
                      : rw.weight;
            mods.Add(w);
            total += w;
        }

        float pick = Random.value * total;
        for (int i = 0; i < mods.Count; i++)
        {
            if (pick < mods[i])
                return rarityWeights[i].rarity;
            pick -= mods[i];
        }
        return rarityWeights[^1].rarity;
    }

    private void ItemPurchasedListener(Item purchased)
    {
        if (!onlyBuyOne) return;

        foreach (var it in spawnedItems)
        {
            if (it == null)
            {
                continue;
            }
            
            if (it != purchased)
            {
                it.currentItemSlot.HidePriceText();
                it.DestroyItem(true);
            }
        }
    }

    public void LeaveShop()
    {
        if (Singleton.Instance.pauseManager.isPaused) return;
        Singleton.Instance.runManager.GoToMap();
    }

    public void DisableOnlyBuyOne()
    {
        onlyBuyOne = false;
        onlyBuyOneIndicator?.SetActive(false);
    }
    
    private void ReRollsIncreasedListener(int value)
    {
        rerollsRemaining += value;
    }
}
