using UnityEngine;

public class SetCabbageVariantEffect : ItemEffect
{
    public CabbageVariantType variant;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        tc.cabbage.SetVariant(variant);
    }

    public override string GetDescription()
    {
        return ($"Turn cabbage into {variant} cabbage");
    }
}
