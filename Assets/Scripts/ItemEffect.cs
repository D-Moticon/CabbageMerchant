using UnityEngine;

[System.Serializable]
public abstract class ItemEffect
{
    public float chance = 1f;

    public virtual void InitializeItemEffect()
    {
        
    }

    public virtual void DestroyItemEffect()
    {
        
    }
    
    public void TryTriggerItemEffect(TriggerContext tc)
    {
        if (chance < 0.999f)
        {
            float rand = Random.Range(0f, 1f);
            if (rand > chance)
            {
                return;
            }
        }
        
        TriggerItemEffect(tc);
    }
    public abstract void TriggerItemEffect(TriggerContext tc);

    public string GetDescriptionWithChance()
    {
        string s = "";
        if (chance < 0.999f)
        {
            s += $"{Helpers.ToPercentageString(chance)} chance to ";
        }

        s += GetDescription();
        return s;
    }
    
    public abstract string GetDescription();

    protected void print(object message)
    {
        Debug.Log(message);
    }
}
