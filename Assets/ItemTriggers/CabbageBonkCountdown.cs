using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CabbageBonkCountdown : Trigger
{
    public double startingCountdown = 1000;
    private double currentCountdown = 1000;

    public enum CountdownType
    {
        numberCabbagesBonked,
        totalBonkValue
    }

    public CountdownType countdownType;
    
    public override void InitializeTrigger(Item item)
    {
        currentCountdown = startingCountdown;
        Cabbage.CabbageBonkedEvent += OnCabbageBonked;
        owningItem.SetExtraText($"{currentCountdown:F0}");
    }

    public override void RemoveTrigger(Item item)
    {
        Cabbage.CabbageBonkedEvent -= OnCabbageBonked;
    }

    public override string GetTriggerDescription()
    {
        switch (countdownType)
        {
            case CountdownType.numberCabbagesBonked:
                return ($"{startingCountdown:F0} cabbages bonked");
            case CountdownType.totalBonkValue:
                return ($"{startingCountdown:F0} bonk value gained");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnCabbageBonked(BonkParams bp)
    {
        switch (countdownType)
        {
            case CountdownType.numberCabbagesBonked:
                currentCountdown -= 1;
                break;
            case CountdownType.totalBonkValue:
                currentCountdown -= bp.totalBonkValueGained;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        owningItem.SetExtraText($"{currentCountdown:F0}");
        
        if (currentCountdown <= 0)
        {
            owningItem.TryTriggerItem();
        }
    }

    public override void RandomizeTrigger()
    {
        base.RandomizeTrigger();
        currentCountdown *= Random.Range(0.1f, 2f);
    }
}
