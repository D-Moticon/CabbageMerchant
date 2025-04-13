using UnityEngine;

public class AddExtraStartingCabbageEffect : ItemEffect
{
    public override void TriggerItemEffect()
    {
        Singleton.Instance.runManager.AddExtraStartingCabbage();
    }
}
