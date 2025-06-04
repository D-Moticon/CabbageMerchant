using UnityEngine;

public class GoldenCabbageValueEffect : ItemEffect
{
    public float valueMult = 2f;
    private bool valueApplied = false;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        
    }

    public override void InitializeItemEffect()
    {
        if (!valueApplied)
        {
            Singleton.Instance.playerStats.goldenCabbageValue *= valueMult;
        }
        
        valueApplied = true;
    }

    public override void DestroyItemEffect()
    {
        if (valueApplied)
        {
            Singleton.Instance.playerStats.goldenCabbageValue /= valueMult;
        }
        
        valueApplied = false;
    }

    public override string GetDescription()
    {
        return ($"Golden cabbages are worth {valueMult:F1}x");
    }
}
