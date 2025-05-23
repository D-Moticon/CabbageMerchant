using UnityEngine;

public class ReplenishSurvivalFoodEffect : ItemEffect
{
    public int foodValue = 6;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.survivalManager.AddFood(foodValue);
    }

    public override string GetDescription()
    {
        return ($"Replenish {foodValue} food in survival mode");
    }
}
