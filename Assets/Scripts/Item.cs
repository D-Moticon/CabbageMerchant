using System;
using UnityEngine;
using System.Collections.Generic;

public class Item : MonoBehaviour, IHoverable
{
    public string itemName;
    public string itemDescription;
    public Sprite icon;

    public enum ItemType
    {
        Item,
        Perk
    };
    public ItemType itemType;
    [SerializeReference] public List<ItemEffect> effects;
    [SerializeReference] public List<Trigger> triggers;
    public Rarity rarity = Rarity.Common;
    public float normalizedPrice = 1f;
    public static float globalItemPriceMult = 10f;
    [HideInInspector] public ItemSlot currentItemSlot;
    [HideInInspector] public ItemWrapper itemWrapper;
    [HideInInspector] public bool purchasable = false;
    public SFXInfo triggerSFX;
    public Item upgradedItem;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void OnEnable()
    {
        foreach (Trigger t in triggers)
        {
            t.owningItem = this;
            t.InitializeTrigger(this);
        }
    }

    protected virtual void OnDisable()
    {
        foreach (Trigger t in triggers)
        {
            t.RemoveTrigger(this);
        }
    }

    public float GetItemBasePrice()
    {
        return (normalizedPrice * globalItemPriceMult);
    }
    
    public virtual void TryTriggerItem(TriggerContext tc = null)
    {
        TriggerItem(tc);
    }

    protected virtual void TriggerItem(TriggerContext tc = null)
    {
        if (itemWrapper == null)
        {
            itemWrapper = GetComponentInParent<ItemWrapper>();
        }
        itemWrapper.triggerFeel.PlayFeedbacks();
        foreach (ItemEffect itemEffect in effects)
        {
            itemEffect.TriggerItemEffect();
        }
        triggerSFX.Play();
    }
    
    public virtual string GetTitleText()
    {
        return itemName;
    }

    public virtual string GetDescriptionText()
    {
        return itemDescription;
    }

    public string GetRarityText()
    {
        return rarity.ToString();
    }

    public string GetTriggerText()
    {
        if (triggers.Count == 0)
        {
            return "";
        }
        
        string s = "Trigger: ";
        
        for (int i = 0; i < triggers.Count; i++)
        {
            s += triggers[i].GetTriggerDescription();
            s += "\n";
        }

        return s;
    }
    
    public Sprite GetImage()
    {
        return icon;
    }

    
}
