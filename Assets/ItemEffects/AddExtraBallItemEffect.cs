using UnityEngine;

public class AddExtraBallItemEffect : ItemEffect
{
    public override void TriggerItemEffect()
    {
        Singleton.Instance.runManager.AddExtraBall();
    }
}
