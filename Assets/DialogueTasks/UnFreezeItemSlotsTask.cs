using System.Collections;
using System.Linq;
using UnityEngine;

public class UnfreezeItemSlotsTask : DialogueTask
{
    [Tooltip("How many random frozen slots to unfreeze")]
    public int numberSlotsToUnfreeze = 1;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        // Grab all currently frozen slots
        var frozenSlots = Singleton.Instance
            .itemManager
            .itemSlots
            .Where(slot => slot.isFrozen)
            .ToList();

        // Don’t try to unfreeze more than we have
        int countToUnfreeze = Mathf.Min(numberSlotsToUnfreeze, frozenSlots.Count);

        for (int i = 0; i < countToUnfreeze; i++)
        {
            // Pick one at random
            int idx = Random.Range(0, frozenSlots.Count);
            var slot = frozenSlots[idx];

            // unfreeze it
            slot.UnFreezeSlot();

            // Remove it so we don’t pick it again
            frozenSlots.RemoveAt(idx);

            // Optional: a small delay so the player can see slots thawing one by one
            yield return new WaitForSeconds(0.25f);
        }

        yield break;
    }
}