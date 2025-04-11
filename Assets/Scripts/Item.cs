using System;
using UnityEngine;
using System.Collections.Generic;

public class Item : MonoBehaviour, IHoverable
{
    public string itemName;
    public Sprite icon;
    [SerializeReference] public List<Trigger> triggers;
    public Rarity rarity = Rarity.Common;
    public float normalizedPrice = 1f;
    public static float globalItemPriceMult = 100f;
    [HideInInspector] public ItemSlot currentItemSlot;
    [HideInInspector] public ItemWrapper itemWrapper;
    [HideInInspector] public bool purchasable = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void OnEnable()
    {
        foreach (Trigger t in triggers)
        {
            t.InitializeTrigger(this);
            t.owningItem = this;
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
    
    protected virtual void TryTriggerItem(TriggerContext tc = null)
    {
        TriggerItem(tc);
    }

    protected virtual void TriggerItem(TriggerContext tc = null)
    {
        itemWrapper.triggerFeel.PlayFeedbacks();
    }

    public class TriggerContext
    {
        public Ball ball;
        public Cabbage cabbage;
        public Vector2 point;
        public Vector2 normal;
    }
    
    //Triggers
    [System.Serializable]
    public abstract class Trigger
    {
        [HideInInspector]public Item owningItem;
        public abstract void InitializeTrigger(Item item);
        public abstract void RemoveTrigger(Item item);
    }

    public class CabbageHitTrigger : Trigger
    {
        public int everyXHit = 1;
        private int hitCounter = 0;
        
        public override void InitializeTrigger(Item item)
        {
            Ball.BallHitCabbageEvent += CabbageHitListener;
            GameStateMachine.EnteringAimStateAction += AimStateEnteredListener;
        }

        public override void RemoveTrigger(Item item)
        {
            Ball.BallHitCabbageEvent -= CabbageHitListener;
            GameStateMachine.EnteringAimStateAction -= AimStateEnteredListener;
        }

        void CabbageHitListener(Ball b, Cabbage c, Vector2 point, Vector2 normal)
        {
            hitCounter++;
            if (hitCounter >= everyXHit)
            {
                TriggerContext tc = new TriggerContext();
                tc.ball = b;
                tc.cabbage = c;
                tc.point = point;
                tc.normal = normal;
                owningItem.TryTriggerItem(tc);
                hitCounter = 0;
            }
        }

        void AimStateEnteredListener()
        {
            hitCounter = 0;
        }
    }

    public class FirstCabbageHitTrigger : Trigger
    {
        bool hit = false;
        
        public override void InitializeTrigger(Item item)
        {
            Ball.BallHitCabbageEvent += CabbageHitListener;
            GameStateMachine.EnteringAimStateAction += AimStateEnteredListener;
        }

        public override void RemoveTrigger(Item item)
        {
            Ball.BallHitCabbageEvent -= CabbageHitListener;
            GameStateMachine.EnteringAimStateAction -= AimStateEnteredListener;
        }

        void CabbageHitListener(Ball b, Cabbage c, Vector2 point, Vector2 normal)
        {
            if (hit)
            {
                return;
            }

            TriggerContext tc = new TriggerContext();
            tc.ball = b;
            tc.cabbage = c;
            tc.point = point;
            tc.normal = normal;
            owningItem.TryTriggerItem(tc);
            hit = true;
            
        }

        void AimStateEnteredListener()
        {
            hit = false;
        }
    }

    public virtual string GetTitleText()
    {
        return itemName;
    }

    public virtual string GetDescriptionText()
    {
        return "";
    }

    public string GetRarityText()
    {
        return rarity.ToString();
    }

    public Sprite GetImage()
    {
        return icon;
    }
}
