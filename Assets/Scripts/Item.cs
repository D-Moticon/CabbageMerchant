using System;
using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;
using Sirenix.OdinInspector;
using TMPro;
using MoreMountains.Feedbacks;

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
        Pet,
        Consumable
    };
    public ItemType itemType;
    public Material customMaterial;
    [SerializeReference]
    public List<MaterialPropertyOverride> materialPropertyOverrides;
    public bool canBeForceTriggered = true;
    [SerializeReference] public List<ItemEffect> effects;
    [SerializeReference] public List<ItemEffect> holofoilEffects;
    public string holofoilEffectDescription;
    [SerializeReference] public List<Trigger> triggers;
    public float triggerChance = 1f;
    public bool hasCooldown = false;
    public int limitPerShot = 0;
    public bool isTemporary = false;
    public bool allowTriggerRandomization = true;
    [ShowIf("@isTemporary == true")]
    public int numberShotsBeforeDestroy = 1;
    private int temporaryCountdown = 1;
    private int timesTriggeredThisShot = 0;
    [ShowIf("@hasCooldown == true")]
    public float cooldownDuration = 1f;
    [HideInInspector]public float cooldownCounter = 0f;
    public Rarity rarity = Rarity.Common;
    public float normalizedPrice = 1f;
    public static float globalItemPriceMult = 10f;
    [HideInInspector] public float sellValueMultiplier = 1.0f;
    [HideInInspector] public ItemSlot currentItemSlot;
    public ItemWrapper itemWrapper;
    [HideInInspector] public bool purchasable = false;
    public SFXInfo triggerSFX;
    public Item upgradedItem;
    public PetDefinition requiredPet;
    public bool survivalModeOnly = false;
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

    [HideInInspector] public bool isMysterious = false;

    public class DestroyItemParams
    {
        public Item item;
        public bool stopDestroy = false;
    }

    public delegate void DestroyItemDelegate(DestroyItemParams dip);

    public static event DestroyItemDelegate DestroyItemPreEvent;
    
    
    private void Update()
    {
        currentFrameTriggerCount = 0;

        if (Singleton.Instance.pauseManager.isPaused)
        {
            return;
        }

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

        if (isHolofoil)
        {
            InitializeHolofoilEffectsExclusive();
        }

        else
        {
            InitializeNonHoloEffectsExclusive();
        }

        GameStateMachine.EnteringAimStateAction += EnteringAimStateListener;
        ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
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
        
        foreach (ItemEffect itemEffect in holofoilEffects)
        {
            itemEffect.DestroyItemEffect();
        }
        
        GameStateMachine.EnteringAimStateAction -= EnteringAimStateListener;
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
    }

    public void SetIcon(Sprite newIcon)
    {
        icon = newIcon;
        if (itemWrapper != null)
        {
            itemWrapper.SetSprite(newIcon);
        }
    }
    
    public void InitializeItemAfterWrapperCreated()
    {
        if (isTemporary)
        {
            MakeTemporary(numberShotsBeforeDestroy);
        }
    }
    
    public void MakeTemporary(int numShots)
    {
        numberShotsBeforeDestroy = numShots;
        temporaryCountdown = numShots;
        isTemporary = true;
        itemWrapper.InitializeItemTemporary();
    }
    
    void InitializeNonHoloEffectsExclusive()
    {
        foreach (ItemEffect itemEffect in effects)
        {
            itemEffect.owningItem = this;
            itemEffect.InitializeItemEffect();
        }
        
        foreach (ItemEffect itemEffect in holofoilEffects)
        {
            itemEffect.owningItem = this;
            itemEffect.DestroyItemEffect();
        }
    }

    void InitializeHolofoilEffectsExclusive()
    {
        foreach (ItemEffect itemEffect in effects)
        {
            itemEffect.owningItem = this;
            itemEffect.DestroyItemEffect();
        }
        
        foreach (ItemEffect itemEffect in holofoilEffects)
        {
            itemEffect.owningItem = this;
            itemEffect.InitializeItemEffect();
        }
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
        if (currentItemSlot != null)
        {
            if (currentItemSlot.isFrozen)
            {
                return;
            }
        }
        
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

        if (limitPerShot > 0 && timesTriggeredThisShot >= limitPerShot)
        {
            return;
        }
        
        TriggerItem(tc);
    }

    protected virtual void TriggerItem(TriggerContext tc = null)
    {
        if (itemWrapper == null)
        {
            Debug.Log($"D: {itemName} item wrapper is null");
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

        timesTriggeredThisShot++;
        currentFrameTriggerCount++;
        
        
    }
    
    public virtual string GetTitleText(HoverableModifier hoverableModifier = null)
    {
        if (isMysterious)
        {
            return "???";
        }
        
        return itemName;
    }

    public virtual string GetDescriptionText(HoverableModifier hoverableModifier = null)
    {
        if (isMysterious)
        {
            return "";
        }

        string s = "";

        if (currentItemSlot != null)
        {
            if (currentItemSlot.isFrozen)
            {
                s += $"<color=red>Item slot frozen. This item will not trigger.</color>";
                s += "\n";
            }
        }
        
        if (!string.IsNullOrEmpty(itemDescription))
        {
            s += itemDescription;
            return s;
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

            if (limitPerShot > 0)
            {
                s += $" (Limit {limitPerShot} use per shot)";
            }

            if (isTemporary)
            {
                s += ("\n" + $"Item is destroyed after {numberShotsBeforeDestroy} shots");
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
            case ItemType.Consumable:
                col = "green";
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
        if (isMysterious)
        {
            return "";
        }
        
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
        if (isMysterious)
        {
            return "";
        }
        
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
            if (!canBeForceTriggered)
            {
                s += "\n";
            }
        }

        if (!canBeForceTriggered)
        {
            s += "(Cannot be force triggered)";
        }

        return s;
    }
    
    public Sprite GetImage()
    {
        if (isMysterious)
        {
            return (itemWrapper.mysteriousSprite);
        }
        
        return icon;
    }

    public string GetValueText()
    {
        if (isMysterious)
        {
            return ("");
        }
        
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
            InitializeHolofoilEffectsExclusive();
        }
    }

    public void SetGhost()
    {
        itemWrapper.spriteRenderer.material = itemWrapper.ghostMaterial;
    }

    public void RandomizeEffectPowers()
    {
        foreach (var effect in effects)
        {
            effect.RandomizePower();
        }
        
        foreach (var effect in holofoilEffects)
        {
            effect.RandomizePower();
        }
    }

    public void RandomizeTriggers()
    {
        if (Singleton.Instance.buildManager.buildMode == BuildManager.BuildMode.release)
        {
            Debug.Log($"Randomized {itemName} triggers:");
        }

        foreach (var trigger in triggers)
        {
            trigger.RandomizeTrigger();
            if (Singleton.Instance.buildManager.buildMode == BuildManager.BuildMode.release)
            {
                Debug.Log($"-{trigger.GetTriggerDescription()}");
            }
        }
    }

    public void SellItem()
    {
        Singleton.Instance.playerStats.AddCoins(GetSellValue());
    }

    public double GetSellValue()
    {
        if (itemType == ItemType.Consumable)
        {
            return 0;
        }
        return (Math.Floor(normalizedPrice * globalItemPriceMult * 0.5*sellValueMultiplier));
    }

    public void DestroyItem(bool withFX = false, bool sendToGraveyard = false)
    {
        //Before destroying, we see if anything wants to prevent the destruction
        DestroyItemParams dip = new DestroyItemParams();
        dip.item = this;
        dip.stopDestroy = false;
        DestroyItemPreEvent?.Invoke(dip);
        if (dip.stopDestroy)
        {
            return;
        }
        
        if (sendToGraveyard)
        {
            if (currentItemSlot != null)
            {
                currentItemSlot.currentItem = null;
            }
            
            Singleton.Instance.itemGraveyard.AddToGraveyard(this, withFX);
        }
        else
        {
            if (currentItemSlot != null)
            {
                currentItemSlot.currentItem = null;
            }
            itemWrapper.DestroyItem(withFX);
        }
    }

    public void EnteringAimStateListener()
    {
        cooldownCounter = -1f;
        timesTriggeredThisShot = 0;
    }

    public void SetNormalizedPrice(float newSellValue)
    {
        normalizedPrice = newSellValue;
        if (currentItemSlot != null)
        {
            currentItemSlot.SetPriceText();
        }
    }

    public void MultiplyNormalizedPrice(float mult)
    {
        normalizedPrice *= mult;
        if (currentItemSlot != null)
        {
            currentItemSlot.SetPriceText();
        }
    }

    public void SetExtraText(string text)
    {
        if (itemWrapper == null)
        {
            Debug.Log($"Tried to set extra text but item wrapper was null: {this.gameObject.name}, {this.itemName}");
        }
        else
        {
            itemWrapper.SetExtraText(text);
        }
    }

    public void MakeItemMysterious()
    {
        isMysterious = true;
        itemWrapper.SetMysterious();
    }

    public void EndItemMysterious()
    {
        isMysterious = false;
        itemWrapper.EndMysterious();
    }

    private void ItemPurchasedListener(Item item)
    {
        if (item != this)
        {
            return;
        }
        
        EndItemMysterious();
    }
    
    private void BallFiredListener(Ball b)
    {
        if (!isTemporary)
        {
            return;
        }

        temporaryCountdown--;
        itemWrapper.UpdateTemporaryCountdown(temporaryCountdown);
        
        if (temporaryCountdown <= 0)
        {
            DestroyItem(true, true);
        }
    }

    public Item GetHighestUpgradedItem()
    {
        Item current = this;
        while (current.upgradedItem != null)
        {
            current = current.upgradedItem;
        }
        return current;
    }
}
