using System;
using UnityEngine;

public class BreakAfterBallCollide : MonoBehaviour
{
    public int numberCollides = 1;
    private int collideCounter = 0;
    public SFXInfo breakSFX;
    public PooledObjectData breakVFX;

    private void OnEnable()
    {
        collideCounter = 0;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Ball b = other.gameObject.GetComponent<Ball>();
        if (b != null)
        {
            collideCounter++;
        }

        if (collideCounter >= numberCollides)
        {
            breakSFX.Play(this.transform.position);
            if (breakVFX != null)
            {
                breakVFX.Spawn(this.transform.position);
            }

            collideCounter = 0;
            
            gameObject.SetActive(false);
        }
    }
}
