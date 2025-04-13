using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemCollection", menuName = "Scriptable Objects/ItemCollection")]
public class ItemCollection : ScriptableObject
{
    [System.Serializable]
    public class ItemInfo
    {
        public Item item;
        public bool enabled = true;
    }
    
    public List<ItemInfo> items;

    /// <summary>
    /// Returns a list of items that match the given rarity.
    /// </summary>
    public List<Item> GetItemsByRarity(Rarity rarity)
    {
        List<Item> matchingItems = new List<Item>();
        foreach (var item in items)
        {
            if (item.item.rarity == rarity && item.enabled)
            {
                matchingItems.Add(item.item);
            }
        }
        return matchingItems;
    }
}