using UnityEngine;

public class GoldenCabbageVariant : CabbageVariant
{
    public FloaterReference goldenHitFloater;
    
    public override void CabbageBonked(Cabbage.CabbageBonkParams cbp)
    {
        base.CabbageBonked(cbp);
        
        float goldValue = Singleton.Instance.playerStats.goldenCabbageValue;
        Singleton.Instance.playerStats.AddCoins(goldValue);
        Singleton.Instance.floaterManager.SpawnFloater(goldenHitFloater, goldValue.ToString(), this.transform.position);
        owningCabbage.SetNoVariant();
    }
}
