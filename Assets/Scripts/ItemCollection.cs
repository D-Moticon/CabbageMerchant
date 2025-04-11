using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemCollection", menuName = "Scriptable Objects/ItemCollection")]
public class ItemCollection : ScriptableObject
{
    public List<Item> items;

    /// <summary>
    /// Returns a list of items that match the given rarity.
    /// </summary>
    public List<Item> GetItemsByRarity(Rarity rarity)
    {
        List<Item> matchingItems = new List<Item>();
        foreach (var item in items)
        {
            if (item.rarity == rarity)
            {
                matchingItems.Add(item);
            }
        }
        return matchingItems;
    }
}