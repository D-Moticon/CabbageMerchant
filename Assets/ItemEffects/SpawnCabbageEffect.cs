using UnityEngine;
using System.Collections.Generic;

public class SpawnCabbageEffect : ItemEffect
{
    public bool ensureNoCabbageOverlap = true;
    public float overlapCheckRadius = 0.5f;
    public List<CabbageVariantType> specialVariants;
    public float specialVariantChance = 0.5f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        BonkableSlot bs =
            GameSingleton.Instance.gameStateMachine.GetEmptyBonkableSlot(ensureNoCabbageOverlap, overlapCheckRadius);
        if (bs == null)
        {
            return;
        }

        Cabbage c = GameSingleton.Instance.gameStateMachine.SpawnCabbageInSlot(bs);
        if (specialVariants != null && specialVariants.Count > 0)
        {
            float rand = Random.Range(0f, 1f);
            if (rand < specialVariantChance)
            {
                int randVar = Random.Range(0, specialVariants.Count);
                c.SetVariant(specialVariants[randVar]);
            }
        }
    }

    public override string GetDescription()
    {
        string variantString = "";
        if (specialVariants != null && specialVariants.Count > 0)
        {
            string vString = specialVariants[0].ToString();
            if (specialVariants.Count > 0)
            {
                for (int i = 1; i < specialVariants.Count; i++)
                {
                    string orString = "";
                    if (i == specialVariants.Count - 1)
                    {
                        orString = " or ";
                    }
                    else
                    {
                        orString = ", ";
                    }
                    vString += $"{orString} {specialVariants[i]}";
                }
            }


            variantString = $" ({Helpers.ToPercentageString(specialVariantChance)} chance to be {vString})";
        }
        
        return ($"Spawn cabbage in a random open space{variantString}");
    }
}
