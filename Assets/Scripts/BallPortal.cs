using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BallPortal : MonoBehaviour
{
    [HideInInspector] public BallPortal otherPortal;
    [HideInInspector] public int bonkValue = 1;

    [Header("Entrance Control")]
    [Tooltip("If false, objects cannot enter/teleport via this portal (but can exit)")]
    public bool canEnter = true;

    [Header("Teleport Speed Limit")]
    [Tooltip("Maximum speed preserved after teleport")]
    public float maxSpeed = 20f;

    [Header("Teleport VFX & SFX")]
    [Tooltip("Spawned at start and end of teleport")]
    public PooledObjectData teleportVFXPrefab;
    [Tooltip("Sound played when a ball teleports")]
    public SFXInfo teleportSFX;

    // track objects recently teleported to prevent immediate return
    private HashSet<GameObject> sentObjects = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D col)
    {
        // if entry is disabled, do nothing
        if (!canEnter)
            return;

        GameObject go = col.gameObject;
        // if this object was just sent here, ignore and clear
        if (sentObjects.Contains(go))
        {
            sentObjects.Remove(go);
            return;
        }

        // entry teleport VFX
        if (teleportVFXPrefab != null)
        {
            var entryVfx = teleportVFXPrefab.Spawn();
            entryVfx.transform.position = go.transform.position;
        }

        // compute teleport destination
        Vector3 offset = go.transform.position - transform.position;
        Vector3 destination = otherPortal.transform.position + offset;
        go.transform.position = destination;

        // exit teleport VFX
        if (teleportVFXPrefab != null)
        {
            var exitVfx = teleportVFXPrefab.Spawn();
            exitVfx.transform.position = destination;
        }

        // clamp velocity if Rigidbody2D present
        if (go.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        }

        // play teleport sound
        teleportSFX?.Play();

        // prevent immediate return through other portal
        otherPortal.sentObjects.Add(go);

        // apply bonk value if Ball
        var ball = go.GetComponent<Ball>();
        if (ball != null)
            ball.AddBonkValue(bonkValue * Singleton.Instance.playerStats.GetWeaponPowerMult());
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        GameObject go = col.gameObject;
        // allow re-entry after exit
        if (sentObjects.Contains(go))
            sentObjects.Remove(go);
    }

    private void OnDestroy()
    {
        sentObjects.Clear();
    }
}