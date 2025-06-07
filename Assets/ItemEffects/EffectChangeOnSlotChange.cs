using UnityEngine;
using System.Collections.Generic;

public class EffectChangeOnSlotChange : ItemEffect
{
    [System.Serializable]
    public class EffectInfo
    {
        public Sprite icon;
        [SerializeReference]
        public ItemEffect itemEffect;
        [SerializeReference]
        public Trigger trigger;
        
    }

    public SFXInfo changeSFX;
    
    [SerializeReference]
    public List<EffectInfo> effectInfos;

    private int currentSlot = -1;
    private EffectInfo currentEffectInfo;

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        ItemManager.ItemAddedToSlotEvent += ItemAddedToSlotListener;
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        ItemManager.ItemAddedToSlotEvent -= ItemAddedToSlotListener;
        
        foreach (var ei in effectInfos)
        {
            if (ei.itemEffect != null)
            {
                ei.itemEffect.DestroyItemEffect();
            }
            
            if (ei.trigger != null)
            {
                ei.trigger.RemoveTrigger(owningItem);
            }
        }
    }
    
    private void ItemAddedToSlotListener(Item item, ItemSlot slot)
    {
        if (owningItem.currentItemSlot == null)
        {
            return;
        }

        if (currentSlot == owningItem.currentItemSlot.slotNumber)
        {
            return;
        }

        if (!Singleton.Instance.itemManager.itemSlots.Contains(owningItem.currentItemSlot))
        {
            //prevent change in store slots
            return;
        }

        changeSFX.Play(owningItem.transform.position);
        currentSlot = owningItem.currentItemSlot.slotNumber;
        if (currentSlot < effectInfos.Count)
        {
            currentEffectInfo = effectInfos[currentSlot];
            owningItem.SetIcon(effectInfos[currentSlot].icon);
        }

        else
        {
            currentEffectInfo = null;
        }
        
        
        for (int i = 0; i < effectInfos.Count; i++)
        {
            if (i == owningItem.currentItemSlot.slotNumber)
            {
                if (effectInfos[i].itemEffect != null)
                {
                    effectInfos[i].itemEffect.owningItem = owningItem;
                    effectInfos[i].itemEffect.InitializeItemEffect();
                }

                if (effectInfos[i].trigger != null)
                {
                    effectInfos[i].trigger.owningItem = owningItem;
                    effectInfos[i].trigger.InitializeTrigger(owningItem);
                }
            }

            else
            {
                if (effectInfos[i].itemEffect != null)
                {
                    effectInfos[i].itemEffect.DestroyItemEffect();
                }

                if (effectInfos[i].trigger != null)
                {
                    effectInfos[i].trigger.RemoveTrigger(owningItem);
                }
                
            }
            
            
        }
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (currentEffectInfo == null)
        {
            return;
        }
        
        currentEffectInfo.itemEffect.TryTriggerItemEffect(tc);
    }

    public override string GetDescription()
    {
        string s = "";
        s += "Effect changes depending on slot.";
        s += "\n";
        
        if (currentSlot < 0)
        {
            return s;
        }

        if (effectInfos[currentSlot].itemEffect.chance < 0.999f)
        {
            s += $"{Helpers.ToPercentageString(effectInfos[currentSlot].itemEffect.chance)} chance to ";
        }
        
        s += effectInfos[currentSlot].itemEffect.GetDescription();
        return s;
    }

    public EffectInfo GetCurrentEffectInfo()
    {
        return currentEffectInfo;
    }

    public override void RandomizePower()
    {
        base.RandomizePower();
        foreach (var effect in effectInfos)
        {
            effect.itemEffect.RandomizePower();
        }
    }
}
