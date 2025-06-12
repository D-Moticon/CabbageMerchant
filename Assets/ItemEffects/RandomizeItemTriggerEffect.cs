using UnityEngine;
using System.Collections.Generic;

public class RandomizeItemTriggerEffect : ItemEffect
{
    [SerializeReference]
    public List<Trigger> triggersToChooseFrom;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Item item = tc.itemA;
        if (item == null)
        {
            return;
        }

        if (item == owningItem)
        {
            return;
        }

        if (item.itemType != Item.ItemType.Item)
        {
            return;
        }

        if (!item.allowTriggerRandomization)
        {
            return;
        }
        
        int rand = Random.Range(0, triggersToChooseFrom.Count);
        Trigger prototype = triggersToChooseFrom[rand];
        Trigger clone     = Helpers.DeepClone(prototype);

        clone.RandomizeTrigger();

        clone.owningItem = item;
        clone.InitializeTrigger(item);

        foreach (Trigger t in item.triggers)
        {
            t.RemoveTrigger(item);
        }
        
        item.triggers.Clear();
        item.triggers.Add(clone);
        item.keepTriggerOnUpgrade = true;
        
    }

    public override string GetDescription()
    {
        return ("Randomize item trigger");
    }
}
