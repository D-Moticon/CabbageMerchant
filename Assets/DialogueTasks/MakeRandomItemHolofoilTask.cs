using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class MakeRandomItemHolofoilTask : DialogueTask
{
    public PooledObjectData holofoilVFX;
    public SFXInfo holofoilSFX;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();
        if (items == null || items.Count == 0) { yield return new WaitForSeconds(1f); yield break; }
        var itemToHolofoil = items[Random.Range(0, items.Count)];
        if (itemToHolofoil != null)
        {
            itemToHolofoil.SetHolofoil();
            holofoilVFX.Spawn(itemToHolofoil.transform.position);
            holofoilSFX.Play();
        }

        yield return new WaitForSeconds(1f);
    }
}
