using System.Collections;
using UnityEngine;

public class GiveRandomItemDialogueTask : DialogueTask
{
    public ItemCollection randomItemCollection;
    public Rarity rarity;
    public DialogueLine randomItemLine;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        var pool = randomItemCollection.GetItemsByRarity(rarity);
        if (pool.Count == 0) { yield break; }
        var prefab = pool[Random.Range(0, pool.Count)];
        var slot = GameObject.Instantiate(Singleton.Instance.itemManager.itemSlotPrefab, dc.dialogueBox.itemSlotParent);
        slot.isEventSlot = true;
        var item = Singleton.Instance.itemManager.GenerateItemWithWrapper(prefab);
        Singleton.Instance.itemManager.AddItemToSlot(item, slot);
        var textTask = new Task(randomItemLine.RunTask(dc));
        while (textTask.Running) yield return null;
        if (slot.currentItem != null)
        {
            Singleton.Instance.itemManager.MoveItemToEmptyInventorySlot(item, 0.25f);
            yield return new WaitForSeconds(0.5f);
        }
        slot.DestroySlot();
    }
}
