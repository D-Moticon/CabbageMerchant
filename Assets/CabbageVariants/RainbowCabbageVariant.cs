using UnityEngine;

public class RainbowCabbageVariant : CabbageVariant
{
    public override void Initialize(Cabbage cabbage)
    {
        base.Initialize(cabbage);
        
        cabbage.AddPointsPerSize(1);
    }
}
