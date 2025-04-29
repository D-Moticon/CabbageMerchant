using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public abstract class Trigger
{
    [HideInInspector]public Item owningItem;
    public abstract void InitializeTrigger(Item item);
    public abstract void RemoveTrigger(Item item);
    public abstract string GetTriggerDescription();
    public virtual void RandomizeTrigger(){}

    [HideInInspector] public bool itemHasTriggeredThisFrame = false;
}