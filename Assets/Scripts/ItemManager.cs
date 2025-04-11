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
        UpdateItemPurchase();
    }

    void UpdateItemPurchase()
    {
        bool slotAvailable = false;
        
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].currentItem == null)
            {
                slotAvailable = true;
                break;
            }
        }

        if (!slotAvailable)
        {
            return;
        }
        
        // 1) Get mouse position in world space
        Vector2 mouseWorldPos = Singleton.Instance.playerInputManager.mousePosWorldSpace;

        // 2) Check if left mouse is clicked
        if (Singleton.Instance.playerInputManager.fireDown)
        {
            // 3) Find any collider at that position
            Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);
            if (col)
            {
                // 4) Attempt to get an Item component
                Item clickedItem = col.GetComponentInChildren<Item>();
                if (clickedItem != null && clickedItem.purchasable)
                {
                    double cost = clickedItem.GetItemBasePrice();
                    // 5) Check if player has enough coins
                    if (Singleton.Instance.playerStats.coins >= cost)
                    {
                        // Remove item from its current slot if any
                        if (clickedItem.currentItemSlot != null)
                        {
                            clickedItem.currentItemSlot.HidePriceText();
                            clickedItem.currentItemSlot.currentItem = null;
                            clickedItem.currentItemSlot = null;
                        }

                        // 6) Find first available item slot
                        for (int i = 0; i < itemSlots.Count; i++)
                        {
                            if (itemSlots[i].currentItem == null)
                            {
                                // 7) Place the item in the slot
                                AddItemToSlot(clickedItem, itemSlots[i]);
                                // 8) Deduct coins
                                Singleton.Instance.playerStats.AddCoins(-cost);;
                                break;
                            }
                        }
                        
                        ItemPurchasedEvent?.Invoke(clickedItem);
                    }
                }
            }
        }
    }

    private void GenerateItemSlots()
    {
        itemSlots.Clear();

        // Calculate total grid width/height in world units
        Vector2 gridSize = new Vector2(columns * spacing.x, rows * spacing.y);

        // We want slot 0 to be the upper-left slot.
        // So, we start from the top-left corner
        // Then move columns to the right and rows downward.

        Vector2 startPos = (Vector2)transform.position + new Vector2(
            -gridSize.x * 0.5f + spacing.x * 0.5f, // shift left half the width, right half a slot
            gridSize.y * 0.5f - spacing.y * 0.5f   // shift up half the height, down half a slot
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Calculate total grid width/height in world units
        Vector2 gridSize = new Vector2(columns * spacing.x, rows * spacing.y);

        // Start from top-left corner
        Vector2 startPos = (Vector2)transform.position + new Vector2(
            -gridSize.x * 0.5f + spacing.x * 0.5f,
            gridSize.y * 0.5f - spacing.y * 0.5f
        );

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = startPos + new Vector2(col * spacing.x, -row * spacing.y);
                Gizmos.DrawWireCube(pos, slotSize);
            }
        }
    }

    public Item GenerateItemWithWrapper(Item itemPrefab, Vector2 pos = default, Transform parent = null)
    {
        ItemWrapper iw = Instantiate(itemWrapperPrefab, pos, Quaternion.identity);
        Item item = Instantiate(itemPrefab, iw.transform);
        item.transform.localPosition = Vector2.zero;
        iw.transform.SetParent(parent);

        // Initialize references directly here
        iw.spriteRenderer.sprite = item.icon;
        iw.item = item;
        item.itemWrapper = iw;

        return item;
    }

    public void AddItemToSlot(Item item, ItemSlot itemSlot)
    {
        itemSlot.currentItem = item;
        item.currentItemSlot = itemSlot;
        item.itemWrapper.transform.position = itemSlot.transform.position;
        item.itemWrapper.transform.parent = itemSlot.transform;
    }
}
