using UnityEngine;

public class IncreaseBallBonkValueByConsumablesUsed : ItemEffect
{
    public float bonkValuePerConsumable = 0.1f;
    private float currentBonkValueAdd = 0f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null || tc.ball == null)
        {
            return;
        }
        
        tc.ball.AddBonkValue(currentBonkValueAdd);
    }

    public override string GetDescription()
    {
        return ($"Balls gain +{bonkValuePerConsumable:F2} bonk value for every food item eaten this run" +
                $" (currently +{currentBonkValueAdd:F2}");
    }

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        PlayerStats.ConsumablesUsedUpdatedEvent += OnConsumableUsed;
        OnConsumableUsed(Singleton.Instance.playerStats.totalConsumablesUsedThisRun);
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        PlayerStats.ConsumablesUsedUpdatedEvent -= OnConsumableUsed;
    }

    void OnConsumableUsed(int totalUsed)
    {
        currentBonkValueAdd = bonkValuePerConsumable*Singleton.Instance.playerStats.totalConsumablesUsedThisRun;
        owningItem.SetExtraText($"+{currentBonkValueAdd:F2}");
    }
    
    
}
