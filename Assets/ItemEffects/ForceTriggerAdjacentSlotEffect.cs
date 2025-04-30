using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Forces the item in an adjacent slot (left, right, up, or down) to trigger its effect.
/// </summary>
public class ForceTriggerAdjacentSlotEffect : ItemEffect
{
    public enum Direction { Left, Right, Up, Down }

    [Tooltip("Which adjacent slot to target.")]
    public Direction direction;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (owningItem == null || owningItem.currentItemSlot == null)
            return;

        // All inventory slots laid out in two columns
        var slots = Singleton.Instance.itemManager.itemSlots;
        const int columns = 2;
        int currentIndex = slots.IndexOf(owningItem.currentItemSlot);
        if (currentIndex < 0)
            return;

        // Determine adjacent index based on direction
        int adjacentIndex = -1;
        switch (direction)
        {
            case Direction.Left:
                if (currentIndex % columns != 0)
                    adjacentIndex = currentIndex - 1;
                break;
            case Direction.Right:
                if (currentIndex % columns != columns - 1)
                    adjacentIndex = currentIndex + 1;
                break;
            case Direction.Up:
                if (currentIndex - columns >= 0)
                    adjacentIndex = currentIndex - columns;
                break;
            case Direction.Down:
                if (currentIndex + columns < slots.Count)
                    adjacentIndex = currentIndex + columns;
                break;
        }

        // Validate and trigger
        if (adjacentIndex < 0 || adjacentIndex >= slots.Count)
            return;

        var adjacentSlot = slots[adjacentIndex];
        var adjItem = adjacentSlot.currentItem;
        if (adjItem != null)
        {
            adjItem.ForceTriggerItem(tc);
        }
    }

    public override string GetDescription()
    {
        string directionString = "";
        switch (direction)
        {
            case Direction.Left:
                directionString = "to the left";
                break;
            case Direction.Right:
                directionString = "to the right";
                break;
            case Direction.Up:
                directionString = "above";
                break;
            case Direction.Down:
                directionString = "below";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return $"Force trigger the item in the slot {directionString}";
    }
}
