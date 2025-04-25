using UnityEngine;

public class ForceTriggerRandomSlotEffect : ItemEffect
{
    public Vector2Int randomSlotRange = new Vector2Int(0, 5);
    public int numberTimes = 1;
    public PooledObjectData tracer;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        int rand = Random.Range(randomSlotRange.x, randomSlotRange.y);
        ItemSlot itemSlot = Singleton.Instance.itemManager.itemSlots[rand];

        if (itemSlot == null || owningItem == null)
        {
            return;
        }
        
        if (tracer != null)
        {
            Tracer.SpawnTracer(tracer, owningItem.transform.position, itemSlot.transform.position, 0.3f, 1f, owningItem.icon, Color.white);
        }

        if (itemSlot.currentItem != null && itemSlot.currentItem.itemType == Item.ItemType.Item && itemSlot.currentItem != owningItem)
        {
            for (int i = 0; i < numberTimes; i++)
            {
                itemSlot.currentItem.ForceTriggerItem(tc);
            }
        }
    }

    public override string GetDescription()
    {
        string s;
        if (randomSlotRange.y - randomSlotRange.x > 0)
        {
            s = $"Force trigger a random item in slots {randomSlotRange.x+1}-{randomSlotRange.y+1}";
        }
        else
        {
            s = $"Force trigger item in slot {randomSlotRange.x+1}";
        }

        if (numberTimes > 1)
        {
            s += $" {numberTimes} times";
        }

        return s;
    }
}
