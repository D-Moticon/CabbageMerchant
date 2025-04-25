using UnityEngine;
using System.Collections.Generic;

public class MakeRandomItemHolofoilEffect : ItemEffect
{
    public PooledObjectData holoVFX;
    public SFXInfo holoSFX;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        List<Item> validItems = Singleton.Instance.itemManager.GetItemsInInventory();
        validItems.Shuffle();
        if (validItems[0] != null)
        {
            validItems[0].SetHolofoil();
            holoVFX.Spawn(validItems[0].transform.position);
            holoSFX.Play(validItems[0].transform.position);
        }
    }

    public override string GetDescription()
    {
        return $"Make one random inventory item holofoil";
    }
}
