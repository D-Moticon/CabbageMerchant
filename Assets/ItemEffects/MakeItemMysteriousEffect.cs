using UnityEngine;

public class MakeItemMysteriousEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc.itemA == null)
        {
            return;
        }
        
        tc.itemA.MakeItemMysterious();
    }

    public override string GetDescription()
    {
        return "";
    }
}
