using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FMOD.Studio;

public class Weapon_Rocket_ItemEffect : ItemEffect
{
    public PooledObjectData rocket;
    public Sprite overrideRocketSprite;
    public float boostForce = 10f;
    public float maxYVel = 15f;
    public float minYVel = -5f;
    public float duration = 1.0f;
    public PooledObjectData explosionPiece;
    public int explosionPieceQuantity = 4;
    public float explosionPieceSpeed = 15f;
    public PooledObjectData explosionVFX;
    public SFXInfo explosionSFX;
    public SFXInfo flightSFX;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        List<Ball> activeBalls = GameSingleton.Instance.gameStateMachine.activeBalls;
        foreach (Ball ball in activeBalls)
        {
            GameSingleton.Instance.gameStateMachine.StartCoroutine(RocketCoroutine(ball));
        }
    }

    public override string GetDescription()
    {
        string s =
            $"Launch all balls upward as rockets that pierce cabbages and explode after {duration:F1}s or when releasing weapon fire button";
        return s;
    }

    IEnumerator RocketCoroutine(Ball ball)
    {
        if (ball == null)
        {
            yield break;
        }
        
        GameObject instantiatedRocket = rocket.Spawn(ball.transform.position, Quaternion.identity);
        if (overrideRocketSprite != null)
        {
            SpriteRenderer sr = instantiatedRocket.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = overrideRocketSprite;
            }
        }
        instantiatedRocket.transform.parent = ball.transform;
        ball.gameObject.layer = LayerMask.NameToLayer("BallWallsOnly");

        FMOD.Studio.EventInstance flightSFXInstance = flightSFX.Play(duration,ball.transform.position);
        
        float elapsedTime = 0f;
        while (elapsedTime < duration && !Singleton.Instance.playerInputManager.weaponFireUp)
        {
            ball.rb.AddForce(boostForce*Vector2.up);
            if (ball.rb.linearVelocity.y > maxYVel)
            {
                ball.rb.linearVelocity = new Vector2(ball.rb.linearVelocity.x, maxYVel);
            }
            if (ball.rb.linearVelocity.y < minYVel)
            {
                ball.rb.linearVelocity = new Vector2(ball.rb.linearVelocity.x, minYVel);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        flightSFXInstance.stop(STOP_MODE.IMMEDIATE);
        
        if (Singleton.Instance.playerInputManager.weaponFireUp && elapsedTime < 0.2f)
        {
            Singleton.Instance.gameHintManager.QueueHintUntilBouncingDone("Looks like I need to hold the weapon button to get the full effect!");
        }

        explosionVFX.Spawn(ball.transform.position);
        explosionSFX.Play();

        if (rocket != null)
        {
            //Return to pool logic
            GameSingleton.Instance.objectPoolManager.ReturnToPool(rocket,instantiatedRocket);
        }
        
        for (int i = 0; i < explosionPieceQuantity; i++)
        {
            Rigidbody2D expPiece = explosionPiece.Spawn(ball.transform.position).GetComponent<Rigidbody2D>();
            float ang = 360f / explosionPieceQuantity * i;
            Vector2 dir = Helpers.AngleDegToVector2(ang);
            Vector2 vel = dir * explosionPieceSpeed;
            expPiece.linearVelocity = vel;
        }
        
        ball.gameObject.layer = LayerMask.NameToLayer("Ball");
    }
}
