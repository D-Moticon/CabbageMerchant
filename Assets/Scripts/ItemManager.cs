using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ItemSlot itemSlotPrefab;
    public ItemWrapper itemWrapperPrefab;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    [Header("Grid Dimensions")]
    public int rows = 4;
    public int columns = 2;

    [Header("Layout Settings")]
    public Vector2 spacing = new Vector2(1f, 1f);
    public Vector2 slotSize = new Vector2(1f, 1f);

    [Header("Starting Items")]
    public List<Item> startingItems = new List<Item>();

    public delegate void ItemPurchasedDelegate(Item item);
    public static event ItemPurchasedDelegate ItemPurchasedEvent;

    [Header("Sell Settings")]
    public Collider2D sellCollider;
    public ParticleSystem sellVFX;
    public SFXInfo sellSFX;
    public FloaterReference sellFloater;

    [Header("Merge Settings")]
    public ParticleSystem mergeVFX;
    public SFXInfo mergeSFX;
    
    [Header("Perk Settings")]
    public Transform perkParent;
    public float perkAreaXSize = 5f;

    // Drag-and-drop
    private Item draggingItem = null;
    private Vector3 draggingStartPos;
    private ItemSlot draggingStartSlot;

    private void Start()
    {
        GenerateItemSlots();
        for (int i = 0; i < startingItems.Count && i < itemSlots.Count; i++)
        {
            Item item = GenerateItemWithWrapper(startingItems[i]);
            AddItemToSlot(item, itemSlots[i]);
        }
    }

    private void Update()
    {
        HandleItemDragging();
        HandleItemHoverAndMerging();
    }

    //==================================
    // DRAG + DROP
    //==================================
    private void HandleItemDragging()
    {
        Vector2 mouseWorldPos = Singleton.Instance.playerInputManager.mousePosWorldSpace;

        if (Singleton.Instance.playerInputManager.fireDown)
        {
            StartDragIfPossible(mouseWorldPos);
        }
        else if (Singleton.Instance.playerInputManager.fireHeld && draggingItem != null)
        {
            // Move item with mouse
            draggingItem.itemWrapper.transform.position = mouseWorldPos;
        }
        else if (Singleton.Instance.playerInputManager.fireUp && draggingItem != null)
        {
            AttemptToPlaceItem(mouseWorldPos);

            // re-enable collider
            Collider2D dragColl = draggingItem.itemWrapper.GetComponent<Collider2D>();
            if (dragColl) dragColl.enabled = true;

            // hide override tooltip
            Singleton.Instance.toolTip.HideTooltip();
            draggingItem = null;
        }
    }

    private void StartDragIfPossible(Vector2 mouseWorldPos)
    {
        if (draggingItem != null) return;

        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);
        if (!col) return;

        Item clickedItem = col.GetComponentInChildren<Item>();
        if (clickedItem == null) return;

        // If perk, single-click buy
        if (clickedItem.itemType == Item.ItemType.Perk)
        {
            HandlePerkClickPurchase(clickedItem);
            return;
        }

        // Otherwise normal item logic:
        if (clickedItem.purchasable)
        {
            double cost = clickedItem.GetItemPrice();
            if (Singleton.Instance.playerStats.coins < cost)
            {
                Debug.Log("Cannot afford this item, cannot drag.");
                return;
            }
        }

        // Start dragging
        draggingItem = clickedItem;
        draggingStartPos = draggingItem.itemWrapper.transform.position;
        draggingStartSlot = draggingItem.currentItemSlot;

        if (draggingStartSlot != null)
        {
            draggingStartSlot.currentItem = null;
            draggingStartSlot.HidePriceText();
            draggingItem.currentItemSlot = null;
        }

        // disable collider
        Collider2D dragColl = draggingItem.itemWrapper.GetComponent<Collider2D>();
        if (dragColl) dragColl.enabled = false;
    }

    private void AttemptToPlaceItem(Vector2 mouseWorldPos)
    {
        // First check if we are dropping on the sellCollider
        if (sellCollider && sellCollider.OverlapPoint(mouseWorldPos))
        {
            // We are dropping onto the Sell zone
            // Only sell if item is owned (not from the shop)
            if (!draggingItem.purchasable)
            {
                // Actually call SellItem
                draggingItem.SellItem();
                sellVFX.transform.position = draggingItem.transform.position;
                sellVFX.Play();
                sellSFX.Play();
                sellFloater.Spawn(draggingItem.GetSellValue().ToString(), draggingItem.transform.position, Color.white);
                
                // Destroy it visually
                Destroy(draggingItem.itemWrapper.gameObject);
            }
            else
            {
                // If not owned, revert
                RevertDraggedItem();
            }
            return;
        }

        // Otherwise check if we are dropping on an inventory slot
        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);
        ItemSlot slot = col ? col.GetComponentInParent<ItemSlot>() : null;

        if (slot == null || !IsInventorySlot(slot))
        {
            // not a valid slot => revert
            RevertDraggedItem();
            return;
        }

        // Now we know it's an inventory slot
        if (slot.currentItem == null)
        {
            // place item
            if (draggingItem.purchasable)
            {
                // This is a newly purchased item
                double cost = draggingItem.GetItemPrice();
                Singleton.Instance.playerStats.AddCoins(-cost);
                draggingItem.purchasable = false;
                ItemPurchasedEvent?.Invoke(draggingItem);
            }
            AddItemToSlot(draggingItem, slot);
        }
        else
        {
            // try merge
            if (CheckForDuplicateMerge(draggingItem, slot.currentItem, slot))
            {
                mergeVFX.transform.position = slot.transform.position;
                mergeVFX.Play();
                mergeSFX.Play();
            }
            else
            {
                RevertDraggedItem();
            }
        }
    }

    //===================================
    // MERGING
    //===================================
    private bool CheckForDuplicateMerge(Item dragged, Item inSlot, ItemSlot slot)
    {
        if (dragged.itemName == inSlot.itemName && dragged.upgradedItem != null)
        {
            Debug.Log("Merging items into upgraded item!");

            bool isHolofoil = inSlot.isHolofoil || dragged.isHolofoil;

            // Destroy old items
            Destroy(inSlot.itemWrapper.gameObject);
            Destroy(dragged.itemWrapper.gameObject);

            // create upgraded
            Item upgraded = GenerateItemWithWrapper(dragged.upgradedItem);
            if (isHolofoil) upgraded.SetHolofoil();
            AddItemToSlot(upgraded, slot);

            // If dragged was from shop => pay cost once
            // We'll also consider the new item as purchased
            if (dragged.purchasable)
            {
                double cost = dragged.GetItemPrice();
                Singleton.Instance.playerStats.AddCoins(-cost);
                dragged.purchasable = false;

                // Fire the purchased event exactly once, referencing the new upgraded item
                ItemPurchasedEvent?.Invoke(upgraded);
            }

            return true;
        }
        return false;
    }

    private void RevertDraggedItem()
    {
        if (draggingItem == null) return;

        if (draggingStartSlot != null)
        {
            AddItemToSlot(draggingItem, draggingStartSlot);
            if (draggingItem.purchasable)
            {
                draggingStartSlot.ShowPriceText();
            }
        }
        else
        {
            // put back in shop or do nothing
            draggingItem.itemWrapper.transform.position = draggingStartPos;
            Debug.Log("Item put back to shop location or left in world space.");
        }
    }

    //===================================
    // PERK LOGIC
    //===================================
    private void HandlePerkClickPurchase(Item perkItem)
    {
        if (perkItem.purchasable)
        {
            double cost = perkItem.GetItemPrice();
            if (Singleton.Instance.playerStats.coins < cost)
            {
                Debug.Log("Cannot afford perk.");
                return;
            }

            Singleton.Instance.playerStats.AddCoins(-cost);
            perkItem.purchasable = false;
            ItemPurchasedEvent?.Invoke(perkItem);
        }

        perkItem.itemWrapper.transform.SetParent(perkParent);
        perkItem.itemWrapper.transform.localPosition = Vector3.zero; 
        if (perkItem.currentItemSlot != null)
        {
            perkItem.currentItemSlot.currentItem = null;
            perkItem.currentItemSlot.HidePriceText();
            perkItem.currentItemSlot = null;
        }

        ItemPurchasedEvent?.Invoke(perkItem);
        DistributePerks();
    }

    //===================================
    // MERGE TOOLTIP
    //===================================
    private void HandleItemHoverAndMerging()
    {
        if (draggingItem == null) return;
        if (draggingItem.upgradedItem == null) return;

        Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        Collider2D col = Physics2D.OverlapPoint(mousePos);
        if (col)
        {
            Item hoveredItem = col.GetComponentInChildren<Item>();
            if (hoveredItem != null)
            {
                if (hoveredItem == draggingItem)
                {
                    Singleton.Instance.toolTip.HideTooltip();
                    return;
                }

                if (hoveredItem.itemName == draggingItem.itemName)
                {
                    // show override
                    HoverableModifier hm = new HoverableModifier();
                    hm.isHolofoil = draggingItem.isHolofoil;
                    Singleton.Instance.toolTip.ShowOverrideTooltip(draggingItem.upgradedItem, hm);
                    return;
                }
            }
        }
        Singleton.Instance.toolTip.HideTooltip();
    }

    //===================================
    // SELLING: HANDLED ABOVE
    //===================================

    //===================================
    // PERK LAYOUT
    //===================================
    private void DistributePerks()
    {
        // gather perk children
        List<Transform> perkChildren = new List<Transform>();
        foreach (Transform child in perkParent)
        {
            if (child.GetComponentInChildren<Item>() != null)
            {
                perkChildren.Add(child);
            }
        }

        if (perkChildren.Count == 0) return;
        float halfWidth = perkAreaXSize * 0.5f;
        float step = perkAreaXSize / (perkChildren.Count + 1);

        for (int i = 0; i < perkChildren.Count; i++)
        {
            float x = -halfWidth + step * (i + 1);
            perkChildren[i].localPosition = new Vector3(x, 0f, 0f);
        }
    }

    //===================================
    // INVENTORY UTILS
    //===================================
    private bool IsInventorySlot(ItemSlot slot)
    {
        return itemSlots.Contains(slot);
    }

    public Item GenerateItemWithWrapper(Item itemPrefab, Vector2 pos = default, Transform parent = null)
    {
        ItemWrapper iw = Instantiate(itemWrapperPrefab, pos, Quaternion.identity);
        Item item = Instantiate(itemPrefab, iw.transform);
        item.transform.localPosition = Vector2.zero;
        iw.transform.SetParent(parent);

        iw.spriteRenderer.sprite = item.icon;
        iw.item = item;
        item.itemWrapper = iw;

        if (item.holofoilEffects != null && item.holofoilEffects.Count > 0)
        {
            float holoRand = UnityEngine.Random.Range(0f, 1f);
            if (holoRand <= Singleton.Instance.playerStats.holofoilChance)
            {
                item.SetHolofoil();
            }
        }

        return item;
    }

    public void AddItemToSlot(Item item, ItemSlot itemSlot)
    {
        itemSlot.currentItem = item;
        item.currentItemSlot = itemSlot;
        item.itemWrapper.transform.position = itemSlot.transform.position;
        item.itemWrapper.transform.SetParent(itemSlot.transform);
    }

    private void GenerateItemSlots()
    {
        itemSlots.Clear();

        Vector2 gridSize = new Vector2(columns * spacing.x, rows * spacing.y);
        Vector2 startPos = (Vector2)transform.position + new Vector2(
            -gridSize.x * 0.5f + spacing.x * 0.5f,
            gridSize.y * 0.5f - spacing.y * 0.5f
        );

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = startPos + new Vector2(col * spacing.x, -row * spacing.y);
                ItemSlot newSlot = Instantiate(itemSlotPrefab, pos, Quaternion.identity, transform);
                itemSlots.Add(newSlot);
            }
        }
    }
}
