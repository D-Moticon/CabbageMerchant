using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

/// <summary>
/// Offers two items from the graveyard to the player; they may drag one into their inventory.
/// </summary>
public class GraveyardItemsTask : DialogueTask
{
    [Tooltip("Dialogue when there are no items in the graveyard.")]
    public DialogueLine noItemsDialogueLine;

    [Tooltip("Optional prompt before selection.")]
    public DialogueLine promptLine;

    public PooledObjectData ghostSpawnVFX;
    public SFXInfo ghostSpawnSFX;
    public SFXInfo pickedSFX;

    [Tooltip("Horizontal spacing between the two slots.")]
    public float slotSpacing = 1.5f;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        // Gather graveyard contents
        var graveItems = Singleton.Instance.itemGraveyard.GetGraveyardContents().ToList();
        if (graveItems.Count == 0)
        {
            if (noItemsDialogueLine != null)
                yield return noItemsDialogueLine.RunTask(dc);
            yield break;
        }

        // Shuffle and take up to two
        for (int i = graveItems.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = graveItems[i];
            graveItems[i] = graveItems[j];
            graveItems[j] = tmp;
        }
        int offerCount = Mathf.Min(2, graveItems.Count);
        var offered = graveItems.GetRange(0, offerCount);

        // Spawn slots and populate
        var slots = new List<ItemSlot>();
        for (int i = 0; i < offerCount; i++)
        {
            float xPos = (i == 0) ? -slotSpacing * 0.5f : slotSpacing * 0.5f;
            var slot = GameObject.Instantiate(
                Singleton.Instance.itemManager.itemSlotPrefab,
                dc.dialogueBox.itemSlotParent);
            slot.transform.localPosition = new Vector3(xPos, 0f, 0f);
            slot.isEventSlot = true;

            // Remove from graveyard and reactivate wrapper
            var graveItem = offered[i];
            Singleton.Instance.itemGraveyard.RemoveFromGraveyard(graveItem);
            if (graveItem.itemWrapper != null)
                graveItem.itemWrapper.gameObject.SetActive(true);

            // Place the item in the slot
            Singleton.Instance.itemManager.AddItemToSlot(graveItem, slot);
            graveItem.SetGhost();
            graveItem.RandomizeEffectPowers();
            graveItem.RandomizeTriggers();

            if (ghostSpawnVFX != null)
                ghostSpawnVFX.Spawn(graveItem.transform.position);

            slots.Add(slot);
        }

        ghostSpawnSFX.Play();

        if (promptLine != null)
            yield return promptLine.RunTask(dc);

        dc.dialogueBox.ActivateButtons(1);
        var passButton = dc.dialogueBox.choiceButtons[0];
        passButton.buttonPressed = false;
        passButton.SetText("Pass");

        var offeredSet = new HashSet<Item>(offered);
        while (true)
        {
            var inventoryItems = Singleton.Instance.itemManager.GetItemsInInventory();
            if (inventoryItems.Any(item => offeredSet.Contains(item)))
                break;
            if (passButton.buttonPressed)
                break;
            yield return null;
        }

        pickedSFX.Play();

        // Return unpicked items to graveyard
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
            {
                slot.currentItem.DestroyItem(true, true);
            }
        }

        dc.dialogueBox.HideAllChoiceButtons();
    }
}
