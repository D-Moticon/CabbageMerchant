using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultiItemChoiceTask : DialogueTask
{
    [Tooltip("How many different items to offer")]
    public int numberChoices = 2;

    [Tooltip("Source collection of possible item prefabs")]
    public ItemCollection itemCollection;

    [Tooltip("Horizontal spacing between each choice slot")]
    public float slotSpacing = 2f;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        // 1) Build & shuffle the prefab pool
        var pool = itemCollection.items; 
        if (pool == null || pool.Count == 0)
            yield break;

        var shuffle = pool
            .OrderBy(_ => Random.value)
            .ToList();
        for (int i = shuffle.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffle[i], shuffle[j]) = (shuffle[j], shuffle[i]);
        }

        // 2) Pick up to `numberChoices`
        int offerCount = Mathf.Min(numberChoices, shuffle.Count);
        var chosen = shuffle.GetRange(0, offerCount);

        // 3) Spawn one event‐slot per choice and place the item
        var slots = new List<ItemSlot>();
        var offeredItems = new List<Item>();
        for (int i = 0; i < offerCount; i++)
        {
            float xOffset = (i - (offerCount - 1) * 0.5f) * slotSpacing;
            var slot = Object.Instantiate(
                Singleton.Instance.itemManager.itemSlotPrefab,
                dc.dialogueBox.itemSlotParent);
            slot.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            slot.isEventSlot = true;
            slots.Add(slot);

            var prefab = chosen[i].item;
            var item  = Singleton.Instance
                            .itemManager
                            .GenerateItemWithWrapper(prefab);
            Singleton.Instance.itemManager.AddItemToSlot(item, slot);
            offeredItems.Add(item);
        }

        // 4) Show a single “Pass” button
        dc.dialogueBox.ActivateButtons(1);
        var passButton = dc.dialogueBox.choiceButtons[0];
        passButton.buttonPressed = false;
        passButton.SetText("Pass");

        var offeredSet = new HashSet<Item>(offeredItems);

        // 5) Wait until the player either:
        //    a) drags one of the offered items into inventory/perks
        //    b) “eats”/sells a consumable (item is destroyed, becomes null)
        //    c) hits the Pass button
        while (true)
        {
            // a) picked into inventory/perk?
            var allHeld = Singleton.Instance
                              .itemManager
                              .GetItemsAndPerks()
                              .Concat(
                                  Singleton.Instance
                                           .itemManager
                                           .GetItemsInInventory()
                              );
            if (allHeld.Any(it => offeredSet.Contains(it)))
                break;

            // b) consumed (destroyed) ?
            if (offeredItems.Any(it => it == null))
                break;

            // c) pass?
            if (passButton.buttonPressed)
                break;

            yield return null;
        }

        // 6) Clean up any remaining offered items & slots
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
                slot.currentItem.DestroyItem(false, true);
            slot.DestroySlot();
        }

        dc.dialogueBox.HideAllChoiceButtons();
    }
}
