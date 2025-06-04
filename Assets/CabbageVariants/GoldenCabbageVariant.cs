using UnityEngine;

public class GoldenCabbageVariant : CabbageVariant
{
    public FloaterReference goldenHitFloater;

    public delegate void GoldenCabbageDelegate(Cabbage c, float goldValue);
    public static GoldenCabbageDelegate GoldenCabbageBonkedEvent;
    
    public override void CabbageBonked(BonkParams bp)
    {
        base.CabbageBonked(bp);
        
        float goldValue = Singleton.Instance.playerStats.goldenCabbageValue*(float)owningCabbage.bonkMultiplier;
        Singleton.Instance.playerStats.AddCoins(goldValue);
        Singleton.Instance.floaterManager.SpawnFloater(goldenHitFloater, $"{goldValue:F0}", this.transform.position);
        owningCabbage.SetNoVariant();
        
        GoldenCabbageBonkedEvent?.Invoke(owningCabbage, goldValue);
    }
}
