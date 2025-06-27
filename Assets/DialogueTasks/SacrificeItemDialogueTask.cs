using System.Collections;
using UnityEngine;

public class SacrificeItemDialogueTask : DialogueTask
{
    [Header("Dialogue Lines")]
    public DialogueLine validItemPlacedLine;    // “You’ve chosen wisely…” etc.
    public DialogueSequenceTask itemSacrificedTask;     // “The item vanishes in a puff of smoke” etc.
    public DialogueSequenceTask noItemsDialogueTask;    // “You have nothing to offer…” etc.

    [Header("Optional VFX/SFX")]
    public PooledObjectData sacrificeVFX;       // e.g. a puff of smoke
    public SFXInfo sacrificeSFX;                // e.g. a mystical chime

    public override IEnumerator RunTask(DialogueContext dc)
    {
        // 1) If no items in inventory, play the “no items” line and quit
        if (Singleton.Instance.itemManager.GetNumberOfItemsInInventory() == 0)
        {
            Task noItems = new Task(noItemsDialogueTask.RunTask(dc));
            while (noItems.Running) yield return null;
            yield break;
        }

        // 2) Prevent selling while we wait for the sacrifice
        Singleton.Instance.itemManager.DisableSell();

        // 3) Spawn an “event” slot and wait for the player to drag an item into it
        ItemSlot sacrificeSlot = CreateItemSlot(dc);
        sacrificeSlot.isEventSlot = true;
        // (you can pass an offset: CreateItemSlot(dc, new Vector2(1.5f,0f)); if you like)
        
        while (sacrificeSlot.currentItem == null)
            yield return null;

        // 4) Player placed something valid — play the “valid” line
        Task validPlaced = new Task(validItemPlacedLine.RunTask(dc));
        while (validPlaced.Running) yield return null;

        // 5) Optional effects at the sacrifice moment
        var sacrificedItem = sacrificeSlot.currentItem;
        if (sacrificeVFX != null)
            sacrificeVFX.Spawn(sacrificedItem.transform.position);
        if (sacrificeSFX != null)
            sacrificeSFX.Play(sacrificedItem.transform.position);

        // 6) Actually destroy the item (removes it from inventory & wrapper)
        Singleton.Instance.itemManager.DestroyItem(sacrificedItem);

        GameObject.Destroy(sacrificeSlot.gameObject);
        
        // 7) Play the “item sacrificed” follow-up line
        Task sacrificedLine = new Task(itemSacrificedTask.RunTask(dc));
        while (sacrificedLine.Running) yield return null;

        // 8) Re-enable selling and finish
        Singleton.Instance.itemManager.EnableSell();
    }
}
