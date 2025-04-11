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

    private void Start()
    {
        GenerateItemSlots();

        for (int i = 0; i < startingItems.Count && i < itemSlots.Count; i++)
        {
            Item item = GenerateItemWithWrapper(startingItems[i]);
            AddItemToSlot(item, itemSlots[i]);
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
                // Each subsequent column is spaced horizontally,
                // each row is spaced downward.
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

    public Item GenerateItemWithWrapper(Item itemPrefab, Vector2 pos = default)
    {
        ItemWrapper iw = Instantiate(itemWrapperPrefab, pos, Quaternion.identity);
        Item item = Instantiate(itemPrefab, iw.transform);
        item.transform.localPosition = Vector2.zero;

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
    }
}
