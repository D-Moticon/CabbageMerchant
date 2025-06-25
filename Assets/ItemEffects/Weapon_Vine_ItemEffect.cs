using UnityEngine;

public class Weapon_Vine_ItemEffect : ItemEffect
{
    [Tooltip("Pooled Vine prefab containing SpriteShapeController and Vine script.")]
    public PooledObjectData vinePooledObject;

    [Tooltip("Maximum time allowed to draw the vine (seconds)")]
    public float vineDrawingDuration = 2f;

    [Tooltip("Lifetime of the vine after drawing finishes (seconds)")]
    public float vineDuration = 5f;

    public float vineBallBonkValueAdd = 0.2f;
    public bool makeRainbow = false;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // Spawn a vine instance
        GameObject vineGO = vinePooledObject.Spawn();
        if (vineGO == null) return;

        // Initialize the Vine script
        Vine vine = vineGO.GetComponent<Vine>();
        if (vine != null)
        {
            vine.Initialize(vineDrawingDuration, vineDuration);
        }

        if (makeRainbow)
        {
            vine.MakeRainbow();
        }

        else
        {
            vine.MakeNonRainbow();
        }
        
        IncreaseBallBonkValueOnCollide ibv = vineGO.GetComponent<IncreaseBallBonkValueOnCollide>();
        ibv.bonkValueAdd = (vineBallBonkValueAdd*Singleton.Instance.playerStats.GetWeaponPowerMult());
    }

    public override string GetDescription()
    {
        return $"Hold weapon fire to draw a vine. When ball bounces off vine, it gains {vineBallBonkValueAdd:F1} * WP bonk power.";
    }
}
