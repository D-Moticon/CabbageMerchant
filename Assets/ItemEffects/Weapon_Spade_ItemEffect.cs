using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Weapon_Spade_ItemEffect : ItemEffect
{
    public float boostSpeed = 30f;
    public float outgoingSpeed = 18f;
    public string extraDescriptionInfo;
    [SerializeReference]
    public ItemEffect ballHitEffect;

    public PooledObjectData ballHitVFX;
    public SFXInfo ballHitSFX;
    
    public float coinChance = 0.1f;
    public PooledObjectData coinVFX;
    public SFXInfo coinSFX;

    public PooledObjectData spadeSprite;

    public class BallSpade
    {
        public Ball ball;
        public GameObject spadeSprite;
    }
    
    public List<BallSpade> ballSpades;
    
    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        Ball.BallHitBonkableEvent += BallHitCabbageListener;
        GameStateMachine.ExitingBounceStateAction += ExitingBounceStateListener;
        ballHitEffect.InitializeItemEffect();
        ballSpades = new List<BallSpade>();
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        Ball.BallHitBonkableEvent -= BallHitCabbageListener;
        GameStateMachine.ExitingBounceStateAction -= ExitingBounceStateListener;
        ballHitEffect.DestroyItemEffect();
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        Ball.BallHitBonkableEvent += BallHitCabbageListener;
        
        List<Ball> activeBalls = GameSingleton.Instance.gameStateMachine.activeBalls;

        if (ballSpades == null)
        {
            ballSpades = new List<BallSpade>();
        }
        
        foreach (BallSpade bs in ballSpades)
        {
            bs.spadeSprite.SetActive(false);
        }
        ballSpades.Clear();
        
        foreach (Ball ball in activeBalls)
        {
            ball.rb.linearVelocity = new Vector2(0f, -boostSpeed);
            GameObject sSprite = spadeSprite.Spawn(ball.transform.position);
            sSprite.transform.SetParent(ball.transform);
            sSprite.transform.localPosition = Vector2.zero;
            sSprite.transform.rotation = quaternion.identity;
            BallSpade bs = new BallSpade();
            bs.ball = ball;
            bs.spadeSprite = sSprite;
            ballSpades.Add(bs);
        }
    }

    public override string GetDescription()
    {
        int numRocks = 0;
        SpawnRBItemEffect srb = (SpawnRBItemEffect)ballHitEffect;
        if (srb != null)
        {
            numRocks = srb.quantity;
        }

        string numString = "";
        if (numRocks > 0)
        {
            numString = numRocks.ToString();
        }
        
        string desc = $"Turn all balls into a spade that launches downward at great speed and explodes into {numString} rocks that bonk cabbages for 2. {extraDescriptionInfo}";
        desc += "\n";
        desc += $"On collision: {Helpers.ToPercentageString(coinChance)} * WP to dig up coin. {extraDescriptionInfo}";
        return desc;
    }

    void BallHitCabbageListener(Ball.BallHitBonkableParams bhp)
    {
        if (bhp == null)
        {
            return;
        }
        
        if (bhp.ball == null)
        {
            return;
        }

        if (ballSpades == null)
        {
            return;
        }
        
        Ball b = bhp.ball;
        
        for (int i = ballSpades.Count - 1; i >= 0; i--)
        {
            if (ballSpades[i].ball == b)
            {
                TriggerContext tc = new TriggerContext();
                tc.ball = b;
                tc.cabbage = bhp.cabbage;
                tc.normal = bhp.normal;
                tc.point = bhp.point;
                ballHitEffect.TriggerItemEffect(tc);
                float coinRand = Random.Range(0f, 1f);
                if (coinRand < coinChance*Singleton.Instance.playerStats.GetWeaponPowerMult())
                {
                    coinVFX.Spawn(tc.point);
                    coinSFX.Play();
                    Singleton.Instance.playerStats.AddCoins(1);
                }
            
                ballHitSFX.Play();
                if (ballHitVFX != null)
                {
                    ballHitVFX.Spawn(bhp.point);
                }

                float ang = Random.Range(70f, 110f);
                Vector2 dir = Helpers.AngleDegToVector2(ang);
                Vector2 vel = outgoingSpeed * dir;
                b.rb.linearVelocity = vel;
                
                Singleton.Instance.screenShaker.ShakeScreen(1f);
                ballSpades[i].spadeSprite.SetActive(false);
                ballSpades.RemoveAt(i);
            }
        }
    }

    void ExitingBounceStateListener()
    {
        if (ballSpades != null)
        {
            foreach (BallSpade bs in ballSpades)
            {
                bs.spadeSprite.SetActive(false);
            }

            ballSpades.Clear();
        }
    }
}
