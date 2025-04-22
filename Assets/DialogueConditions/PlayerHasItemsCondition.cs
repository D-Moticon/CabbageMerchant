using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerHasItemsCondition : DialogueCondition
{
    public int quantity = 1;
    public bool normalItemsOnly = false;
    
    public override bool IsConditionMet()
    {
        List<Item> items;
        
        if (normalItemsOnly)
        {
            items = Singleton.Instance.itemManager.GetNormalItems();
        }
        
        else
        {
            items = Singleton.Instance.itemManager.GetItemsAndPerks();
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
