using UnityEngine;

public class SurvivalModeFoodEffect : ItemEffect
{
    public int foodValue = 6;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        //The food value gets accounted for in survival manager item consumption listen
    }

    public override string GetDescription()
    {
        return ($"Replenish +{foodValue} food in Survival Mode");
    }
}
