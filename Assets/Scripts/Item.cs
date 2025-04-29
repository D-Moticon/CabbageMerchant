using System;
using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;
using Sirenix.OdinInspector;

public class Item : MonoBehaviour, IHoverable
{
    public string itemName;
    public string itemDescription;
    public Sprite icon;

    public enum ItemType
    {
        Item,
        Perk,
        Weapon,
        Pet
    };
    public ItemType itemType;
    public bool canBeForceTriggered = true;
    [SerializeReference] public List<ItemEffect> effects;
    [SerializeReference] public List<ItemEffect> holofoilEffects;
    public string holofoilEffectDescription;
    [SerializeReference] public List<Trigger> triggers;
    public float triggerChance = 1f;
    public bool hasCooldown = false;
    [ShowIf("@hasCooldown == true")]
    public float cooldownDuration = 1f;
    [HideInInspector]public float cooldownCounter = 0f;
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
    public static event ItemDelegate WeaponTriggeredEvent;

    private static int triggerPerFrameLimit = 5;
    private int currentFrameTriggerCount = 0;
    [TextArea] public string firstUseHint;
    private bool hasBeenUsedOnce = false;
    //If this item is part of an upgrade merge, remove the upgraded item's triggers and replace with this item's triggers using Helpers.DeepClone
    [HideInInspector] public bool keepTriggerOnUpgrade = false;

    private void Update()
    {
        currentFrameTriggerCount = 0;

        if (hasCooldown && cooldownCounter > 0)
        {
            if (itemType == ItemType.Weapon)
            {
                cooldownCounter -= (Time.deltaTime * Singleton.Instance.playerStats.weaponCooldownSpeedMult);
            }

            else
            {
                cooldownCounter -= Time.deltaTime;
            }
            
        }

        foreach (var trigger in triggers)
        {
            trigger.itemHasTriggeredThisFrame = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void OnEnable()
    {
        foreach (Trigger t in triggers)
        {
            t.owningItem = this;
            t.InitializeTrigger(this);
        }

        foreach (ItemEffect itemEffect in effects)
        {
            itemEffect.owningItem = this;
            itemEffect.InitializeItemEffect();
        }
        
        foreach (ItemEffect itemEffect in holofoilEffects)
        {
            itemEffect.owningItem = this;
            itemEffect.InitializeItemEffect();
        }

        GameStateMachine.EnteringAimStateAction += EnteringAimStateListener;
    }

    protected virtual void OnDisable()
    {
        foreach (Trigger t in triggers)
        {
            t.RemoveTrigger(this);
        }
        
        foreach (ItemEffect itemEffect in effects)
        {
            itemEffect.DestroyItemEffect();
        }
        
        GameStateMachine.EnteringAimStateAction -= EnteringAimStateListener;
    }

    public float GetItemPrice()
    {
        return (normalizedPrice * globalItemPriceMult * Singleton.Instance.playerStats.shopDiscountMult);
    }

    public void ForceTriggerItem(TriggerContext tc = null)
    {
        if (canBeForceTriggered)
        {
            TryTriggerItem(tc);
        }
    }
    
    public virtual void TryTriggerItem(TriggerContext tc = null)
    {
        if (currentFrameTriggerCount > triggerPerFrameLimit)
        {
            return;
        }

        if (hasCooldown && cooldownCounter > 0f)
        {
            return;
        }
        
        if (triggerChance < 0.999f)
        {
            float trigRand = UnityEngine.Random.Range(0f, 1f);
            if (trigRand > triggerChance)
            {
                return;
            }
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
                itemEffect.TryTriggerItemEffect(tc);
            }
        }

        else
        {
            foreach (ItemEffect itemEffect in effects)
            {
                itemEffect.TryTriggerItemEffect(tc);
            } 
        }

        foreach (var trigger in triggers)
        {
            trigger.itemHasTriggeredThisFrame = true;
        }

        cooldownCounter = cooldownDuration;
        
        triggerSFX.Play();
        if (itemType == ItemType.Weapon)
        {
            GameSingleton.Instance.gameStateMachine.launcher.launchFeel.PlayFeedbacks();
        }
        ItemTriggeredEvent?.Invoke(this);

        if (itemType == ItemType.Weapon)
        {
            WeaponTriggeredEvent?.Invoke(this);
        }

        if (!hasBeenUsedOnce)
        {
            if (!string.IsNullOrEmpty(firstUseHint))
            {
                Singleton.Instance.gameHintManager.QueueHintUntilBouncingDone(firstUseHint);
            }
            hasBeenUsedOnce = true;
        }
        
        currentFrameTriggerCount++;
        
        
    }
    
    public virtual string GetTitleText(HoverableModifier hoverableModifier = null)
    {
         return itemName;
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

            if (triggerChance < 0.999f)
            {
                s += $"{Helpers.ToPercentageString(triggerChance)} chance to ";
            }
            
            for (int i = 0; i < effectsToUse.Count; i++)
            {
                if (i > 0)
                {
                    s += "\n";
                }
                s += effectsToUse[i].GetDescriptionWithChance();
            }

            if (isHolofoil || (hoverableModifier!=null && hoverableModifier.isHolofoil))
            {
                if (!String.IsNullOrEmpty(holofoilEffectDescription))
                {
                    s += "\n" + $"<rainb>{holofoilEffectDescription}</rainb>";
                }
            }
            
            return s;
        }
    }

    public string GetTypeText(HoverableModifier hoverableModifier = null)
    {
        string col = "";
        switch (itemType)
        {
            case ItemType.Item:
                col = "white";
                break;
            case ItemType.Perk:
                col = "#b10096";
                break;
            case ItemType.Weapon:
                col = "#ff7800";
                break;
            case ItemType.Pet:
                col = "#7ae3ff";
                break;
            default:
                break;
        }

        
        if (isHolofoil || (hoverableModifier != null && hoverableModifier.isHolofoil))
        {
            if (holofoilEffects != null && holofoilEffects.Count > 0)
            {
                return($"<rainb>Holofoil</rainb> <color={col}>{itemType.ToString()}</color>");
            }
            
            else
            {
                return($"<color={col}>{itemType.ToString()}</color>");
            }
        }

        else
        {
            return($"<color={col}>{itemType.ToString()}</color>");
        }
    }

    public string GetRarityText()
    {
        switch (rarity)
        {
            case Rarity.Common:
                return "Common";
            case Rarity.Rare:
                return "<color=#dc00ff><wave a=0.3>Rare";
            case Rarity.Legendary:
                return "<color=#ff9b00><wave a=0.3>Legendary";
            default:
                throw new ArgumentOutOfRangeException();
        }
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

        if (hasCooldown)
        {
            s += $"({Helpers.RoundToDecimal(cooldownDuration, 1)}s cooldown)";
        }

        return s;
    }
    
    public Sprite GetImage()
    {
        return icon;
    }

    public string GetValueText()
    {
        if (itemType == ItemType.Pet)
        {
            return "";
        }

        else
        {
            return (GetSellValue().ToString());
        }
        
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

    public void EnteringAimStateListener()
    {
        cooldownCounter = -1f;
    }
}
