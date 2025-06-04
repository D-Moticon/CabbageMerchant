using System.Collections;
using System.Linq;
using UnityEngine;

public class FreezeItemSlotsTask : DialogueTask
{
    [Tooltip("How many random unfrozen slots to freeze")]
    public int numberSlotsToFreeze = 1;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        // grab all currently unfrozen slots
        var unfrozenSlots = Singleton.Instance
            .itemManager
            .itemSlots
            .Where(slot => !slot.isFrozen)
            .ToList();

        // clamp so we don't try to freeze more than exist
        int countToFreeze = Mathf.Min(numberSlotsToFreeze, unfrozenSlots.Count);

        for (int i = 0; i < countToFreeze; i++)
        {
            // pick one at random
            int idx = Random.Range(0, unfrozenSlots.Count);
            var slot = unfrozenSlots[idx];

            // freeze it
            slot.FreezeSlot();

            // remove from list so we don’t pick it again
            unfrozenSlots.RemoveAt(idx);

            yield return new WaitForSeconds(0.25f);
        }

        // done immediately
        yield break;
    }
}