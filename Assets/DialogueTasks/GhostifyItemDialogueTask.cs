using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GhostifyRandomItemDialogueTask : DialogueTask
{
    public PooledObjectData ghostifyVFX;
    public SFXInfo ghostifySFX;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();
        
        if (items == null || items.Count == 0) { yield return new WaitForSeconds(1f); yield break;}
        
        var item = items[Random.Range(0, items.Count)];
        
        if (item != null)
        {
            item.SetGhost();
            item.RandomizeEffectPowers();

            if (ghostifyVFX != null)
            {
                ghostifyVFX.Spawn(item.transform.position);
            }

            ghostifySFX.Play(item.transform.position);
        }
    }
}
