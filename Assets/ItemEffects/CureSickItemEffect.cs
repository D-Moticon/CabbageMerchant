using UnityEngine;

public class CureSickItemEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.survivalManager.CureSick();
    }

    public override string GetDescription()
    {
        return ("Cure dysentery");
    }
}
