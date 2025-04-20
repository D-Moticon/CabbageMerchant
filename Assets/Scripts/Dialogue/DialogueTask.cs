using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class DialogueTask
{
    public abstract IEnumerator RunTask(DialogueContext dc);

    public static ItemSlot CreateItemSlot(DialogueContext dc, Vector2 offset = default)
    {
        ItemSlot itemSlot = GameObject.Instantiate(Singleton.Instance.itemManager.itemSlotPrefab);
        itemSlot.transform.parent = dc.dialogueBox.itemSlotParent;
        itemSlot.transform.localPosition = new Vector3(0f,0f,0f);
        itemSlot.isEventSlot = true;
        return itemSlot;
    }
}
