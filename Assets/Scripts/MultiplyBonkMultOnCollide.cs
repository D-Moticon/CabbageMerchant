using System;
using UnityEngine;

public class MultiplyBonkMultOnCollide : MonoBehaviour
{
    public float bonkMultMult = 1.1f;
    public PooledObjectData vfx;
    public SFXInfo sfx;

    private void OnTriggerEnter2D(Collider2D other)
    {
        CollisionOccured(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CollisionOccured(other.collider);
    }

    void CollisionOccured(Collider2D other)
    {
        Cabbage c = other.GetComponent<Cabbage>();
        if (c == null)
        {
            return;
        }
        
        c.MultiplyBonkMultiplier(bonkMultMult);
        if (vfx != null)
        {
            vfx.Spawn(c.transform.position);
        }

        sfx.Play();
    }
}
