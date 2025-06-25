using UnityEngine;

public class GhostifyItemEffect : ItemEffect
{
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc?.itemA == null) return;

        var item = tc.itemA;
        item.SetGhost(); // visually & logically marks the item as a ghost
        item.RandomizeEffectPowers();
        Singleton.Instance.itemManager.MoveItemToEmptyInventorySlot(item);
    }

    public override string GetDescription()
    {
        return $"If an item is destroyed (except by selling), instead turn it into a ghost item with randomized effect power.";
    }
}