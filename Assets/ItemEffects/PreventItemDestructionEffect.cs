using UnityEngine;

public class PreventItemDestructionEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        
    }

    public override string GetDescription()
    {
        return ($"Prevent item destruction");
    }

    public override void InitializeItemEffect()
    {
        Item.DestroyItemPreEvent += DestroyItemPreListener;
    }

    public override void DestroyItemEffect()
    {
        Item.DestroyItemPreEvent -= DestroyItemPreListener;
    }
    
    private void DestroyItemPreListener(Item.DestroyItemParams dip)
    {
        float rand = Random.value;
        if (rand < chance)
        {
            dip.stopDestroy = true;
        }
    }
}
