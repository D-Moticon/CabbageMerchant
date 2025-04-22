using System;
using UnityEngine;

public class BallKiller : MonoBehaviour
{
    public PooledObjectData killVFX;
    public SFXInfo killSFX;

    private void OnCollisionEnter2D(Collision2D other)
    {
        Ball b = other.gameObject.GetComponentInChildren<Ball>();
        if (b != null)
        {
            killVFX.Spawn(other.GetContact(0).point);
            killSFX.Play();
            b.KillBall();
        }
    }
}
