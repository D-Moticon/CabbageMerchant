using UnityEngine;

[System.Serializable]
public abstract class ItemEffect
{
    public abstract void TriggerItemEffect(TriggerContext tc);
    public abstract string GetDescription();

    protected void print(object message)
    {
        Debug.Log(message);
    }
}
