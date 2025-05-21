using UnityEngine;
using System.Collections.Generic;

public class DestroyRandomItemEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();

        if (items == null || items.Count == 0)
        {
            return;
        }
        
        int rand = Random.Range(0, items.Count);
        Item itemToDestroy = items[rand];
        if(itemToDestroy != null)
        {
            Singleton.Instance.itemManager.DestroyItem(itemToDestroy, true);
        }
    }

    public override string GetDescription()
    {
        return ("Destroy a random item");
    }
}
