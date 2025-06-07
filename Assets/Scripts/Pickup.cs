using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pickup : MonoBehaviour
{
    public PooledObjectData pickupVFX;
    public SFXInfo pickupSFX;

    // How far to nudge each step when resolving overlap
    [Tooltip("Distance to push pickup each frame until no longer overlapping a cabbage.")]
    public float pushStep = 0.1f;

    private Collider2D _pickupCollider;

    public delegate void PickupDelegate(Pickup p);
    public static PickupDelegate pickupCollectedEvent;
    
    private void Awake()
    {
        _pickupCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
        // If we hit a cabbage, resolve overlap
        var cabbage = other.GetComponent<Cabbage>();
        if (cabbage != null)
        {
            var cabCollider = cabbage.GetComponent<Collider2D>();
            if (cabCollider != null)
            {
                StartCoroutine(ResolveOverlap(cabCollider));
            }
        }

        // If we hit a ball, collect
        var ball = other.GetComponent<Ball>();
        if (ball == null)
            return;
        
        if (pickupVFX != null)
            pickupVFX.Spawn(transform.position);
        if (pickupSFX != null)
            pickupSFX.Play();

        gameObject.SetActive(false);
        
        pickupCollectedEvent?.Invoke(this);
    }

    private IEnumerator ResolveOverlap(Collider2D cabCollider)
    {
        // Push this pickup away until it's no longer overlapping the cabbage
        while (_pickupCollider.IsTouching(cabCollider))
        {
            // Direction from cabbage center to pickup center
            Vector2 dir = (Vector2)_pickupCollider.bounds.center - (Vector2)cabCollider.bounds.center;
            if (dir.sqrMagnitude < 0.0001f)
            {
                // Fallback direction
                dir = Vector2.up;
            }
            dir.Normalize();

            // Nudge position
            transform.position += (Vector3)(dir * pushStep);

            // Wait a frame
            yield return null;
        }
    }
}