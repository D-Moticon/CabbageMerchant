using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weapon_Cholla_Effect : ItemEffect
{
    [Header("Pooled Objects")]
    public PooledObjectData counterObject;
    public PooledObjectData spikeObject;
    public PooledObjectData splodeVFX;
    public SFXInfo splodeSFX;

    [Header("Spike Settings")]
    public float spikeSpeed = 10f;
    public float bounceToSpikeMultiplier = 1f;

    [Header("Ball Jump Settings")]
    public Vector2 jumpSpeedRange = new Vector2(8f, 15f);

    // Track each ball's bounce count and UI
    private Dictionary<Ball, int> bounceCounters = new();
    private Dictionary<Ball, GameObject> counterObjects = new();
    private Dictionary<Ball, TMP_Text> counterTexts = new();

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        Ball.BallEnabledEvent += OnBallSpawned;
        Ball.BallCollidedEvent += OnBallBounced;
        GameStateMachine.ExitingBounceStateAction += OnExitBounceState;
    }

    public override void DestroyItemEffect()
    {
        base.DestroyItemEffect();
        Ball.BallEnabledEvent -= OnBallSpawned;
        Ball.BallCollidedEvent -= OnBallBounced;
        GameStateMachine.ExitingBounceStateAction -= OnExitBounceState;
        CleanupAllCounters();
    }

    private void OnBallSpawned(Ball ball)
    {
        GameObject counterGO = counterObject.Spawn(ball.transform.position, Quaternion.identity);
        counterGO.transform.SetParent(ball.transform, false);
        counterGO.transform.localPosition = new Vector3(0f,0.5f,0f);

        TMP_Text text = counterGO.GetComponentInChildren<TMP_Text>();
        text.text = "0";

        bounceCounters[ball] = 0;
        counterObjects[ball] = counterGO;
        counterTexts[ball] = text;
    }

    private void OnBallBounced(Ball ball, Collision2D collision)
    {
        if (!bounceCounters.ContainsKey(ball))
            return;

        bounceCounters[ball]++;
        counterTexts[ball].text = bounceCounters[ball].ToString();
        owningItem.SetExtraText($"<color=green>" +
                                $"{(bounceCounters[ball]* bounceToSpikeMultiplier * Singleton.Instance.playerStats.GetWeaponPowerMult()):F0}");
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        foreach (var pair in bounceCounters)
        {
            Ball ball = pair.Key;
            int bounceCount = pair.Value;

            if (!ball || !ball.gameObject.activeInHierarchy) continue;

            int spikeCount = Mathf.RoundToInt(bounceCount * bounceToSpikeMultiplier * Singleton.Instance.playerStats.GetWeaponPowerMult());
            if (spikeCount <= 0) continue;

            Vector2 center = ball.transform.position;
            float angleStep = 360f / spikeCount;

            for (int i = 0; i < spikeCount; i++)
            {
                float angle = i * angleStep;
                Vector2 dir = Helpers.AngleDegToVector2(angle);

                GameObject spike = spikeObject.Spawn(center, Quaternion.identity);
                if (spike.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.linearVelocity = dir * spikeSpeed;
                }
            }

            float randYVel = Random.Range(jumpSpeedRange.x, jumpSpeedRange.y);
            ball.rb.linearVelocity = new Vector2(ball.rb.linearVelocity.x, randYVel);

            if (splodeVFX != null)
            {
                splodeVFX.Spawn(ball.transform.position);
            }

            splodeSFX.Play(ball.transform.position);
        }

        CleanupAllCounters();
    }

    private void CleanupAllCounters()
    {
        foreach (var pair in counterObjects)
        {
            GameObject counterGO = pair.Value;
            if (counterGO != null)
            {
                counterGO.transform.SetParent(null);
                Singleton.Instance.objectPoolManager.ReturnToPool(counterObject, counterGO);
            }
        }

        bounceCounters.Clear();
        counterObjects.Clear();
        counterTexts.Clear();
        owningItem.SetExtraText("");
    }

    public override string GetDescription()
    {
        return $"Balls track bounces. When triggered, each ball fires {bounceToSpikeMultiplier}x WP spikes per bounce in all directions.";
    }
    
    private void OnExitBounceState()
    {
        CleanupAllCounters();
    }
}
