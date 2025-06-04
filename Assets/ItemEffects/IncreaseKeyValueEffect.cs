using UnityEngine;

public class IncreaseKeyValueEffect : ItemEffect
{
    public int keyValueIncrease = 1;
    private bool increaseApplied = false;
    
    public override void InitializeItemEffect()
    {
        if (!increaseApplied)
        {
            Singleton.Instance.playerStats.keyValue += keyValueIncrease;
        }
        
        increaseApplied = true;
    }

    public override void DestroyItemEffect()
    {
        if (increaseApplied)
        {
            Singleton.Instance.playerStats.keyValue -= keyValueIncrease;
        }
        
        increaseApplied = false;
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        
    }

    public override string GetDescription()
    {
        return ($"Picking up a key grants {keyValueIncrease} extra key.");
    }
}
