using System;
using UnityEngine;

public class BoostGate : MonoBehaviour
{
    [Tooltip("The speed along the gate's local red (X) axis after passing through")]
    public float gateSpeed = 10f;

    public PooledObjectData boostVFX;
    public SFXInfo boostSFX;

    void Reset()
    {
        // ensure the collider is a trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // grab the Rigidbody2D (if any)
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        // local red axis = transform.right
        Vector2 axis = transform.right.normalized;
        Vector2 vel  = rb.linearVelocity;

        // decompose current velocity
        float   along = Vector2.Dot(vel, axis);         // component along axis
        Vector2 perp  = vel - axis * along;             // remainder

        // build new velocity: perpendicular stays, axis component set to gateSpeed
        rb.linearVelocity = perp + axis * gateSpeed;

        if (boostVFX != null)
        {
            boostVFX.Spawn(rb.transform.position);
            boostSFX.Play(rb.transform.position);
        }
    }
}
