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
    [SerializeReference] public List<ItemEffect> holofoilEffects;
    [SerializeReference] public List<Trigger> triggers;
    public Rarity rarity = Rarity.Common;
    public float normalizedPrice = 1f;
    public static float globalItemPriceMult = 10f;
    [HideInInspector] public float sellValueMultiplier = 1.0f;
    [HideInInspector] public ItemSlot currentItemSlot;
    [HideInInspector] public ItemWrapper itemWrapper;
    [HideInInspector] public bool purchasable = false;
    public SFXInfo triggerSFX;
    public Item upgradedItem;
    [HideInInspector] public bool isHolofoil = false;

    public delegate void ItemDelegate(Item item);

    public static event ItemDelegate ItemTriggeredEvent;

    private static int triggerPerFrameLimit = 5;
    private int currentFrameTriggerCount = 0;

    private void Update()
    {
        currentFrameTriggerCount = 0;
    }

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

    public float GetItemPrice()
    {
        return (normalizedPrice * globalItemPriceMult * Singleton.Instance.playerStats.shopDiscountMult);
    }
    
    public virtual void TryTriggerItem(TriggerContext tc = null)
    {
        if (currentFrameTriggerCount > triggerPerFrameLimit)
        {
            return;
        }
        
        TriggerItem(tc);
    }

    protected virtual void TriggerItem(TriggerContext tc = null)
    {
        if (itemWrapper == null)
        {
            itemWrapper = GetComponentInParent<ItemWrapper>();
        }
        itemWrapper.triggerFeel.PlayFeedbacks();

        if (isHolofoil)
        {
            foreach (ItemEffect itemEffect in holofoilEffects)
            {
                itemEffect.TriggerItemEffect(tc);
            }
        }

        else
        {
            foreach (ItemEffect itemEffect in effects)
            {
                itemEffect.TriggerItemEffect(tc);
            } 
        }
        
        triggerSFX.Play();
        ItemTriggeredEvent?.Invoke(this);
        currentFrameTriggerCount++;
    }
    
    public virtual string GetTitleText(HoverableModifier hoverableModifier = null)
    {
        if (isHolofoil || (hoverableModifier!=null && hoverableModifier.isHolofoil))
        {
            if (holofoilEffects != null && holofoilEffects.Count > 0)
            {
                return($"{itemName} <size=4><rainb>Holofoil</rainb>");
            }

            else
            {
                return itemName;
            }
        }

        else
        {
            return itemName;
        }
        
    }

    public virtual string GetDescriptionText(HoverableModifier hoverableModifier = null)
    {
        if (!string.IsNullOrEmpty(itemDescription))
        {
            return itemDescription;
        }

        else
        {
            List<ItemEffect> effectsToUse;
            if (isHolofoil || (hoverableModifier!=null && hoverableModifier.isHolofoil))
            {
                if (holofoilEffects != null && holofoilEffects.Count > 0)
                {
                    effectsToUse = holofoilEffects;
                }

                else
                {
                    effectsToUse = effects;
                }
            }

            else
            {
                effectsToUse = effects;
            }
            
            string s = "";
            for (int i = 0; i < effectsToUse.Count; i++)
            {
                if (i > 0)
                {
                    s += "\n";
                }
                s += effectsToUse[i].GetDescription();
            }

            return s;
        }
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
        
        else if (triggers.Count == 1)
        {
            if (triggers[0] is ItemAddedTrigger)
            {
                return "";
            }
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

    public string GetValueText()
    {
        return (GetSellValue().ToString());
    }

    public void SetHolofoil()
    {
        if (holofoilEffects != null && holofoilEffects.Count > 0)
        {
            isHolofoil = true;
            itemWrapper.spriteRenderer.material = itemWrapper.holofoilMaterial;
        }
    }

    public void SellItem()
    {
        Singleton.Instance.playerStats.AddCoins(GetSellValue());
    }

    public double GetSellValue()
    {
        
        return (Math.Floor(normalizedPrice * globalItemPriceMult * 0.5*sellValueMultiplier));
    }

    public void DestroyItem(bool withFX = false)
    {
        itemWrapper.DestroyItem(withFX);
    }
}
