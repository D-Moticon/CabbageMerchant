using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class DestroyRandomItemDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();
        if (items == null || items.Count == 0) { yield return new WaitForSeconds(1f); yield break; }
        var itemToDestroy = items[Random.Range(0, items.Count)];
        if (itemToDestroy != null) Singleton.Instance.itemManager.DestroyItem(itemToDestroy, true);

        yield return new WaitForSeconds(1f);
    }
}
