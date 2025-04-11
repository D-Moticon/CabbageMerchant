using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;

    public delegate void BallCabbageDelegate(Ball b, Cabbage c);
    public static BallCabbageDelegate BallHitCabbageEvent;
    
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
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            c.Bonk(other.contacts[0].point);
            BallHitCabbageEvent?.Invoke(this, c);
        }
    }
}
