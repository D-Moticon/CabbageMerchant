using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerHasItemsCondition : DialogueCondition
{
    public int quantity = 1;
    public ItemSlot.AllowedTypes allowedTypes;
    
    public override bool IsConditionMet()
    {
        List<Item> items = null;

        switch (allowedTypes)
        {
            case ItemSlot.AllowedTypes.any:
                items = Singleton.Instance.itemManager.GetItemsConsumablesAndPerks();
                break;
            case ItemSlot.AllowedTypes.itemOnly:
                items = Singleton.Instance.itemManager.GetNormalItems();
                break;
            case ItemSlot.AllowedTypes.perkOnly:
                break;
            case ItemSlot.AllowedTypes.weaponOnly:
                items = Singleton.Instance.itemManager.GetWeapons();
                break;
            case ItemSlot.AllowedTypes.itemOrConsumable:
                items = Singleton.Instance.itemManager.GetNormalItemsAndConsumables();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (items.Count > 0)
        {
            return true;
        }

        else
        {
            return false;
        }
    }
}
