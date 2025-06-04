using UnityEngine;

public class EffectChangeOnSlotTriggerDescription : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        
    }

    public override void RemoveTrigger(Item item)
    {
        
    }

    public override string GetTriggerDescription()
    {
        EffectChangeOnSlotChange ec = owningItem.effects[0] as EffectChangeOnSlotChange;
        if (ec == null)
        {
            return "";
        }

        EffectChangeOnSlotChange.EffectInfo ei = ec.GetCurrentEffectInfo();
        
        if (ei == null)
        {
            return "";
        }

        if (ei.trigger == null)
        {
            return "";
        }

        return (ei.trigger.GetTriggerDescription());
    }
}
