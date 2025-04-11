using System;
using UnityEngine;
using System.Collections.Generic;

public class Item : MonoBehaviour
{
    [SerializeReference] public List<Trigger> triggers;
    
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
    protected virtual void TryTriggerItem(TriggerContext tc = null)
    {
        TriggerItem(tc);
    }

    protected virtual void TriggerItem(TriggerContext tc = null)
    {
        
    }

    public class TriggerContext
    {
        public Ball ball;
        public Cabbage cabbage;
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

        void CabbageHitListener(Ball b, Cabbage c)
        {
            hitCounter++;
            if (hitCounter >= everyXHit)
            {
                TriggerContext tc = new TriggerContext();
                tc.ball = b;
                tc.cabbage = c;
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

        void CabbageHitListener(Ball b, Cabbage c)
        {
            if (hit)
            {
                return;
            }

            TriggerContext tc = new TriggerContext();
            tc.ball = b;
            tc.cabbage = c;
            owningItem.TryTriggerItem(tc);
            hit = true;
            
        }

        void AimStateEnteredListener()
        {
            hit = false;
        }
    }
}
