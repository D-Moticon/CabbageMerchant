using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;

    public class BallHitCabbageParams
    {
        public Ball ball;
        public Cabbage cabbage;
        public Vector2 point;
        public Vector2 normal;
        public float hangTimeBeforeHit;
    }
    
    public delegate void BallCabbageDelegate(BallHitCabbageParams bcParams);
    public static BallCabbageDelegate BallHitCabbageEvent;
    private static float timeoutDuration = 0.5f;
    private static float timeoutVel = 0.1f;
    private float timeoutCounter = 0f;
    public SFXInfo wallBonkSFX;
    public float bonkCooldown = 0.05f;
    private float bonkCooldownCounter;
    private float currentHangtime = 0f;
    
    private void OnEnable()
    {
        GameSingleton.Instance.gameStateMachine.AddActiveBall(this);
    }

    private void OnDisable()
    {
        GameSingleton.Instance.gameStateMachine.RemoveActiveBall(this);
    }

    public void SetVelocity(Vector2 vel)
    {
        rb.linearVelocity = vel;
    }

    private void Update()
    {
        if (rb.position.y < -6)
        {
            KillBall();
        }

        if (rb.linearVelocity.magnitude < timeoutVel)
        {
            timeoutCounter += Time.deltaTime;
            if (timeoutCounter > timeoutDuration)
            {
                KillBall();
            }
        }

        else
        {
            timeoutCounter = 0f;
        }

        if (bonkCooldownCounter > 0f)
        {
            bonkCooldownCounter -= Time.deltaTime;
        }

        currentHangtime += Time.deltaTime;
    }

    void KillBall()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null && bonkCooldownCounter <= 0f)
        {
            c.Bonk(1f, other.contacts[0].point);

            BallHitCabbageParams bcParams = new BallHitCabbageParams();
            bcParams.ball = this;
            bcParams.cabbage = c;
            bcParams.point = other.GetContact(0).point;
            bcParams.normal = other.GetContact(0).normal;
            bcParams.hangTimeBeforeHit = currentHangtime;
            
            BallHitCabbageEvent?.Invoke(bcParams);
            bonkCooldownCounter = bonkCooldown;
            currentHangtime = 0f;
        }

        else
        {
            wallBonkSFX.Play(other.GetContact(0).point);
        }
    }
}
