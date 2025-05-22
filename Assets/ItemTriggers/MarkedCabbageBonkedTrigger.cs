using UnityEngine;

public class MarkedCabbageBonkedTrigger : Trigger
{
    public bool onlyMarksGeneratedByOwningItem = true;
    
    public override void InitializeTrigger(Item item)
    {
        BonkMarker.MarkedCabbageBonkedEvent += MarkedCabbageBonkedListener;
    }

    public override void RemoveTrigger(Item item)
    {
        BonkMarker.MarkedCabbageBonkedEvent -= MarkedCabbageBonkedListener;
    }

    public override string GetTriggerDescription()
    {
        return ("Marked cabbage bonked by ball");
    }
    
    private void MarkedCabbageBonkedListener(BonkParams bp)
    {
        if (onlyMarksGeneratedByOwningItem && bp.causingItem != owningItem)
        {
            return;
        }
        
        TriggerContext tc = new TriggerContext();
        tc.cabbage = bp.bonkedCabbage;
        tc.point = bp.collisionPos;
        
        owningItem.TryTriggerItem(tc);
    }
}
