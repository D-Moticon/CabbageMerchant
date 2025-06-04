using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemsInMultipleSlotsTriggered : Trigger
{
    [Tooltip("Which slot indices to count triggers for (0-based)")]
    public List<int> slotsForTrigger = new List<int>();

    [Tooltip("Total number of hits across those slots to fire this trigger")]
    public int quantity = 1;

    private int currentCount = 0;
    private static readonly Vector2Int randomizeQuantityRange = new Vector2Int(1, 100);

    public override void InitializeTrigger(Item item)
    {
        // reset count whenever the effect is applied
        currentCount = 0;
        Item.ItemTriggeredEvent += OnItemTriggered;
        GameStateMachine.BallFiredEvent += OnBallFired;
    }

    public override void RemoveTrigger(Item item)
    {
        Item.ItemTriggeredEvent -= OnItemTriggered;
        GameStateMachine.BallFiredEvent -= OnBallFired;
    }

    public override string GetTriggerDescription()
    {
        // e.g. "Items in slots 1, 3, 4 triggered 5 times"
        var humanSlots = slotsForTrigger.Select(i => (i + 1).ToString());
        string s = string.Join(", ", humanSlots);
        string plural = quantity > 1 ? "s" : "";
        return $"Items in slots {s} triggered {quantity} time{plural}";
    }

    private void OnItemTriggered(Item item)
    {
        // guard against recursive triggers
        if (itemHasTriggeredThisFrame || item == owningItem) return;

        // must be in one of our watched slots
        var slot = item.currentItemSlot;
        if (slot == null) return;

        var allSlots = Singleton.Instance.itemManager.itemSlots;
        int idx = allSlots.IndexOf(slot);
        if (!slotsForTrigger.Contains(idx)) return;

        // only count during ball‐bounce
        if (!(GameSingleton.Instance.gameStateMachine.currentState is GameStateMachine.BouncingState))
            return;

        currentCount++;
        if (currentCount >= quantity)
        {
            owningItem.TryTriggerItem();
            currentCount = 0;
        }
    }

    private void OnBallFired(Ball b)
    {
        // reset if the player takes a new shot
        currentCount = 0;
    }

    public override void RandomizeTrigger()
    {
        // pick a random quantity
        quantity = Random.Range(randomizeQuantityRange.x, randomizeQuantityRange.y);

        // then pick 1 or 2 random slots to watch
        var slots = Singleton.Instance.itemManager.itemSlots;
        int slotCount = slots.Count;
        slotsForTrigger.Clear();

        // always at least one
        slotsForTrigger.Add(Random.Range(0, slotCount));

        // 50% chance to watch a second slot
        if (slotCount > 1 && Random.value < 0.5f)
        {
            int other;
            do { other = Random.Range(0, slotCount); }
            while (slotsForTrigger.Contains(other));
            slotsForTrigger.Add(other);
        }
    }
}
