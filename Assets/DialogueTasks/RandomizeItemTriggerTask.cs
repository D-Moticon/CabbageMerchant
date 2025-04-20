using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RandomizeItemTriggerTask : DialogueTask
{
    [SerializeReference]
    public List<Trigger> triggersToChooseFrom;
    public DialogueLine validItemPlacedLine;
    public DialogueLine itemChangedLine;
    public PooledObjectData itemChangedVFX;
    public SFXInfo itemChangedSFX;
    
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        ItemSlot itemSlot = DialogueTask.CreateItemSlot(dc);
        itemSlot.allowedTypes = ItemSlot.AllowedTypes.itemOnly;
        
        while (itemSlot.currentItem == null || itemSlot.currentItem.itemType != Item.ItemType.Item)
        {
            yield return null;
        }

        Item item = itemSlot.currentItem;
        
        Task validDialogueTask = new Task(validItemPlacedLine.RunTask(dc));
        while (validDialogueTask.Running)
        {
            yield return null;
        }

        int rand = Random.Range(0, triggersToChooseFrom.Count);
        Trigger prototype = triggersToChooseFrom[rand];
        Trigger clone     = Helpers.DeepClone(prototype);

        clone.RandomizeTrigger();

        clone.owningItem = item;
        clone.InitializeTrigger(item);

        foreach (Trigger t in item.triggers)
        {
            t.RemoveTrigger(item);
        }
        
        item.triggers.Clear();
        item.triggers.Add(clone);

        itemChangedVFX.Spawn(item.transform.position);
        itemChangedSFX.Play();
        
        Task itemChangedTask = new Task(itemChangedLine.RunTask(dc));
        while (itemChangedTask.Running)
        {
            yield return null;
        }
        
        Singleton.Instance.itemManager.MoveItemToEmptyInventorySlot(item, 0.25f);
        yield return new WaitForSeconds(0.5f);
        
        
    }
}
