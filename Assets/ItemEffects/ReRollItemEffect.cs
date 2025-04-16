using UnityEngine;

public class ReRollItemEffect : ItemEffect
{
    public int extraReRolls = 1;
    public int reRollCostReduce = 1;
    public float allHoloChanceAdd = 0f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        print("REROLLFX");
        Singleton.Instance.playerStats.IncreaseReRolls(extraReRolls);
        Singleton.Instance.playerStats.ReduceReRollCost(reRollCostReduce);
        Singleton.Instance.playerStats.AddAllHolofoilRollChance(allHoloChanceAdd);
    }

    public override string GetDescription()
    {
        string plural = extraReRolls > 1 ? "" : "s";
        string extraString = $"{extraReRolls} extra reroll{plural} in shops";
        if (reRollCostReduce > 0)
        {
            extraString += ". ";
        }
        string costString = $"Reroll cost reduced by {reRollCostReduce}";

        string allHoloString = "";
        if (allHoloChanceAdd > 0.0001f)
        {
            allHoloString = $". +{Helpers.ToPercentageString(allHoloChanceAdd)} chance to roll all holofoils";
        }
        
        return ($"{extraString}{costString}{allHoloString}");
    }
}
