using UnityEngine;

public class MakeItemTemporaryEffect : ItemEffect
{
    public int numberTurnsBeforeDestroy = 10;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null)
        {
            return;
        }
        
        if (tc.itemA == null)
        {
            return;
        }

        if (tc.itemA.itemType != Item.ItemType.Item && tc.itemA.itemType != Item.ItemType.Weapon && tc.itemA.itemType != Item.ItemType.Consumable)
        {
            return;
        }

        if (tc.itemA.isTemporary)
        {
            if (numberTurnsBeforeDestroy < tc.itemA.numberShotsBeforeDestroy)
            {
                //Only make the temporary shorter
                tc.itemA.MakeTemporary(numberTurnsBeforeDestroy);
            }
        }


        else
        {
            tc.itemA.MakeTemporary(numberTurnsBeforeDestroy);
        }
        
    }

    public override string GetDescription()
    {
        return $"Make item temporary ({numberTurnsBeforeDestroy} shots)";
    }
}
