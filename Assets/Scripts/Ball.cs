using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;

    public delegate void BallCabbageDelegate(Ball b, Cabbage c, Vector2 point, Vector2 normal);
    public static BallCabbageDelegate BallHitCabbageEvent;
    private static float timeoutDuration = 0.5f;
    private static float timeoutVel = 0.1f;
    private float timeoutCounter = 0f;
    public SFXInfo wallBonkSFX;
    
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

    }

    void KillBall()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            c.Bonk(other.contacts[0].point);
            BallHitCabbageEvent?.Invoke(this, c, other.GetContact(0).point, other.GetContact(0).normal);
        }

        else
        {
            wallBonkSFX.Play(other.GetContact(0).point);
        }
    }
}
