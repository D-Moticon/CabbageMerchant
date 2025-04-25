using UnityEngine;

public class GoldenCabbageVariant : CabbageVariant
{
    public FloaterReference goldenHitFloater;
    
    public override void CabbageBonked(BonkParams bp)
    {
        base.CabbageBonked(bp);
        
        float goldValue = Singleton.Instance.playerStats.goldenCabbageValue;
        Singleton.Instance.playerStats.AddCoins(goldValue*owningCabbage.bonkMultiplier);
        Singleton.Instance.floaterManager.SpawnFloater(goldenHitFloater, goldValue.ToString(), this.transform.position);
        owningCabbage.SetNoVariant();
    }
}
