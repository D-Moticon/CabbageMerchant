using UnityEngine;
using System.Collections.Generic;

public class SpawnCabbageEffect : ItemEffect
{
    [Tooltip("Should ensure spawned cabbages don't overlap existing ones.")]
    public bool ensureNoCabbageOverlap = true;

    [Tooltip("Radius to check for overlaps when spawning cabbages.")]
    public float overlapCheckRadius = 0.5f;

    [Tooltip("Number of cabbages to spawn.")]
    public int quantity = 1;

    [Tooltip("Special cabbage variants to choose from.")]
    public List<CabbageVariantType> specialVariants;

    [Tooltip("Chance (0 to 1) to use a special variant for each spawn.")]
    public float specialVariantChance = 0.5f;

    [Tooltip("Tracer effect data.")]
    public PooledObjectData tracer;

    [Tooltip("Sprite used by the tracer effect.")]
    public Sprite tracerSprite;

    [Tooltip("Color of the tracer effect.")]
    public Color tracerColor = Color.white;

    [Tooltip("Spawn VFX to play at each cabbage position.")]
    public PooledObjectData spawnVFX;

    [Tooltip("Spawn SFX to play per cabbage.")]
    public SFXInfo spawnSFX;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (GameSingleton.Instance.gameStateMachine.currentState is GameStateMachine.ScoringState
            || GameSingleton.Instance.gameStateMachine.currentState is BossFightManager.BossPhaseBeatenState)
        {
            return;
        }
        
        for (int i = 0; i < Mathf.Max(1, quantity); i++)
        {
            BonkableSlot bs = GameSingleton.Instance.gameStateMachine
                .GetEmptyBonkableSlot(ensureNoCabbageOverlap, overlapCheckRadius);
            if (bs == null)
            {
                // no more slots available
                break;
            }

            Cabbage c = GameSingleton.Instance.gameStateMachine.SpawnCabbageInSlot(bs);
            if (c == null)
            {
                continue;
            }

            // possibly assign a special variant
            if (specialVariants != null && specialVariants.Count > 0 
                && Random.Range(0f, 1f) < specialVariantChance)
            {
                int randVar = Random.Range(0, specialVariants.Count);
                c.SetVariant(specialVariants[randVar]);
            }

            // spawn VFX & SFX
            spawnVFX?.Spawn(c.transform.position);
            spawnSFX?.Play();

            // tracer from owning item to cabbage
            if (tracer != null && owningItem != null)
            {
                Tracer.SpawnTracer(
                    tracer,
                    owningItem.transform.position,
                    c.transform.position,
                    0.5f,
                    1f,
                    tracerSprite,
                    tracerColor
                );
            }
        }
    }

    public override string GetDescription()
    {
        // build variant description
        string variantString = "";
        if (specialVariants != null && specialVariants.Count > 0)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(specialVariants[0]);
            for (int i = 1; i < specialVariants.Count; i++)
            {
                sb.Append(i == specialVariants.Count - 1 ? " or " : ", ");
                sb.Append(specialVariants[i]);
            }
            variantString = $" ({Helpers.ToPercentageString(specialVariantChance)} chance to be {sb})";
        }

        // choose singular or plural
        if (quantity <= 1)
        {
            return $"Spawn a cabbage in a random open space{variantString}";
        }
        else
        {
            return $"Spawn {quantity} cabbages in random open spaces{variantString}";
        }
    }
    
    public override void RandomizePower()
    {
        base.RandomizePower();
        quantity = Random.Range(1, 4);
    }
}
