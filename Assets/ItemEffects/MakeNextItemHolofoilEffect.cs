using UnityEngine;

public class MakeNextItemHolofoilEffect : ItemEffect
{
    public int stacksToAdd = 1;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.temporaryBonusManager.AddHolofoilStacks(stacksToAdd);
    }

    public override string GetDescription()
    {
        if (stacksToAdd == 1)
        {
            return ("Make next item holofoil");
        }

        else
        {
            return ($"Make next {stacksToAdd} items holofoil");
        }
    }
}
