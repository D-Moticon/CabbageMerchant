using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Replaces one random active cabbage with a high-value yarn ball.
/// </summary>
public class ConvertRandomCabbage : ItemEffect
{
    [Tooltip("Pool data for the replacement yarn ball.")]
    public PooledObjectData replacementObject;

    public string objectDescriptionString;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        GameStateMachine gsm = GameSingleton.Instance.gameStateMachine;

        if (gsm == null)
        {
            return;
        }

        if (GameSingleton.Instance != null)
        {
            if (GameSingleton.Instance.currentBiomeParent.preventCabbageConversion)
            {
                return;
            }
        }
        
        
        
        // only consider slots where a cabbage is present
        List<BonkableSlot> validSlots = gsm.bonkableSlots
            .Where(slot => slot.bonkable != null)
            .ToList();

        if (validSlots.Count == 0) return;

        // pick a random cabbage slot
        int rand = Random.Range(0, validSlots.Count);
        BonkableSlot bs = validSlots[rand];
        bs.bonkable.Remove();

        // spawn the yarn ball at the cabbage position
        Vector3 spawnPos = bs.transform.position;
        IBonkable newBonk = replacementObject.Spawn(spawnPos, Quaternion.identity).GetComponent<IBonkable>();
        bs.bonkable = newBonk;
        if (newBonk is Cabbage)
        {
            GameSingleton.Instance.gameStateMachine.AddActiveCabbage((Cabbage)newBonk);
        }

        // clear this slot's cabbage reference
        bs.bonkable = null;
    }
    
    public override string GetDescription()
    {
        return $"Convert one random cabbage into {objectDescriptionString}";
    }
}