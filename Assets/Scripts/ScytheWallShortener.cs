using System;
using UnityEngine;

/// <summary>
/// When the scythe’s trigger hits a wall, this will shrink the connected
/// DistanceJoint2D so the chain “retracts” instead of locking up on impact.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ScytheWallShortener : MonoBehaviour
{
    [Tooltip("The DistanceJoint2D that lives on the ball and holds the scythe at a fixed distance.")]
    public DistanceJoint2D joint;

    [Tooltip("Which layers count as walls here.")]
    public LayerMask wallMask;

    [Tooltip("How many world-units per second to retract the chain.")]
    public float shortenSpeed = 5f;

    [Tooltip("Never let the chain get shorter than this.")]
    public float minDistance = 0.5f;

    Collider2D _col;

    void OnEnable()
    {
        // make sure our collider is a trigger
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;

        // if you didn’t wire this in the Inspector, try grabbing it from the parent
        if (joint == null)
            joint = GetComponentInParent<DistanceJoint2D>();
    }

    private void Update()
    {
        if (joint == null)
            joint = GetComponentInParent<DistanceJoint2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (joint == null)
        {
            return;
        }
        
        // only shorten if we’re overlapping a wall
        if (((1 << other.gameObject.layer) & wallMask.value) != 0)
        {
            // retract chain
            float newDist = joint.distance - shortenSpeed * Time.deltaTime;
            joint.distance = Mathf.Max(minDistance, newDist);
        }
    }
}