using UnityEngine;

public class AllowHybridMergeEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.playerStats.AllowHybridMerging();
    }

    public override string GetDescription()
    {
        return ("Items of different levels can now be combined");
    }
}
