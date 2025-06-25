using UnityEngine;

public class FlameLevelEffect : ItemEffect
{
    public float flameLevelAdd = 1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AddFlameLevel(flameLevelAdd);
    }

    public override string GetDescription()
    {
        return ($"Increase bonk power of flames by {flameLevelAdd:F1}");
    }
}
