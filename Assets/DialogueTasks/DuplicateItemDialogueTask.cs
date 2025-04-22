using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class DuplicateItemDialogueTask : DialogueTask
{
    public DialogueLine validItemPlacedLine;
    public PooledObjectData duplicateVFX;
    public DialogueLine itemDuplicatedLine;
    public SFXInfo duplicateSFX;
    public DialogueLine noItemsDialogueLine;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        if (Singleton.Instance.itemManager.GetNumberOfItemsInInventory() == 0)
        {
            Task noItemsDialogueTask = new Task(noItemsDialogueLine.RunTask(dc));
            while (noItemsDialogueTask.Running)
            {
                yield return null;
            }

            yield break;
        }
        
        Singleton.Instance.itemManager.DisableSell();
        
        ItemSlot itemSlot = GameObject.Instantiate(Singleton.Instance.itemManager.itemSlotPrefab);
        itemSlot.transform.parent = dc.dialogueBox.itemSlotParent;
        itemSlot.transform.localPosition = new Vector3(0f,0f,0f);
        itemSlot.isEventSlot = true;
        
        while (itemSlot.currentItem == null)
        {
            yield return null;
        }
        
        Task validDialogueTask = new Task(validItemPlacedLine.RunTask(dc));

        while (validDialogueTask.Running)
        {
            yield return null;
        }
        
        ItemSlot itemSlot2 = GameObject.Instantiate(Singleton.Instance.itemManager.itemSlotPrefab);
        itemSlot2.transform.parent = dc.dialogueBox.itemSlotParent;
        itemSlot2.transform.localPosition = new Vector3(1.05f,0f,0f);
        itemSlot.transform.localPosition = new Vector3(-1.05f,0f,0f);

        Item duplicateItem = Singleton.Instance.itemManager.GenerateItemWithWrapper(itemSlot.currentItem, itemSlot2.transform.position);
        Singleton.Instance.itemManager.AddItemToSlot(duplicateItem, itemSlot2);

        duplicateVFX.Spawn((itemSlot2.transform.position+itemSlot.transform.position)*0.5f);
        duplicateSFX.Play();
        
        Task itemDupedTask = new Task(itemDuplicatedLine.RunTask(dc));
        while (itemDupedTask.Running)
        {
            yield return null;
        }
        
        if (itemSlot.currentItem != null || itemSlot2.currentItem != null)
        {
            List<Item> items = new List<Item>();
            if (itemSlot.currentItem != null)
            {
                items.Add(itemSlot.currentItem);
            }

            if (itemSlot2.currentItem != null)
            {
                items.Add(itemSlot2.currentItem);
            }

            Singleton.Instance.itemManager.MoveItemsToEmptyInventorySlots(items, 0.25f);
            yield return new WaitForSeconds(0.5f);
        }
        
        Singleton.Instance.itemManager.EnableSell();
    }

}
