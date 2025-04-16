using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;
using System.Collections;

public class SpawnRBItemEffect : ItemEffect
{
    public PooledObjectData objectToSpawn;

    public enum SpawnLocation
    {
        ball,
        worldArea
    }

    public SpawnLocation spawnLocation;
    
    [ShowIf("@spawnLocation == SpawnLocation.worldArea")]
    public Vector2 spawnCenter;
    
    [ShowIf("@spawnLocation == SpawnLocation.worldArea")]
    public Vector2 spawnAreaSize = new Vector2(1f, 1f);
    
    public string objectName;
    public string objectDescription;
    public int quantity = 1;
    static float colliderDisableDuration = 0.1f;
    public Vector2 speedRange = new Vector2(5f, 10f);
    public float spreadAngle = 45f;
    public Vector2 scaleRange = new Vector2(1f, 1f);

    public enum SpreadType
    {
        even,
        random
    }

    public SpreadType spreadType;
    public float surfaceOffset = 0.35f;
    public PooledObjectData spawnVFX;
    public bool rotateVFXtoNormal = false;
    
    // NEW: Define the enum for velocity direction
    public enum VelocityDirection
    {
        Normal,         // Use TriggerContext.normal (default)
        WorldDirection  // Use a custom direction input
    }

    [EnumToggleButtons]
    public VelocityDirection velocityDirection = VelocityDirection.Normal;

    [ShowIf("@velocityDirection == VelocityDirection.WorldDirection")]
    public Vector2 worldVelocityDirection;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // Use the trigger context values:
        Vector2 pos = Vector2.zero;
        Vector2 normal = Vector2.up;

        switch (velocityDirection)
        {
            case VelocityDirection.Normal:
                normal = tc.normal;
                break;
            case VelocityDirection.WorldDirection:
                normal = worldVelocityDirection.normalized;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        for (int i = 0; i < quantity; i++)
        {
            switch (spawnLocation)
            {
                case SpawnLocation.ball:
                    pos = tc.point;
                    break;
                case SpawnLocation.worldArea:
                    pos = spawnCenter - spawnAreaSize * 0.5f + new Vector2(Random.Range(0f, spawnAreaSize.x), Random.Range(0f, spawnAreaSize.y));
                    break;
                default:
                    break;
            }
            
            float scaRand = Random.Range(scaleRange.x, scaleRange.y);
            
            Rigidbody2D rb = objectToSpawn.Spawn(pos + normal * surfaceOffset).GetComponent<Rigidbody2D>();
            rb.position = pos + normal * surfaceOffset;
            rb.transform.localScale = new Vector3(scaRand, scaRand, 1f);
            
            Vector2 dir = Vector2.up;
            float ang = 0f;

            switch (spreadType)
            {
                case SpreadType.even:
                    float startAngle = Helpers.Vector2ToAngle(normal) - (spreadAngle * 0.5f);
                    if (quantity == 1)
                    {
                        ang = Helpers.Vector2ToAngle(normal);
                    }
                    else
                    {
                        float angleStep = spreadAngle / (quantity - 1);
                        ang = startAngle + angleStep * i;
                    }
                    break;
                case SpreadType.random:
                    ang = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f) + Helpers.Vector2ToAngle(normal);
                    break;
                default:
                    break;
            }
            
            dir = Helpers.AngleDegToVector2(ang);
            
            float randSpeed = Random.Range(speedRange.x, speedRange.y);
            Vector2 vel = randSpeed * dir;
            rb.linearVelocity = vel;
            
            // ——— DISABLE COLLIDER BRIEFLY ———
            Collider2D col = rb.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                GameSingleton.Instance.StartCoroutine(ReenableCollider(col, colliderDisableDuration));
            }
        }

        if (spawnVFX != null)
        {
            GameObject vfx = spawnVFX.Spawn(pos);

            if (rotateVFXtoNormal)
            {
                Quaternion rot = Helpers.Vector2ToRotation(normal);
                vfx.transform.rotation = rot;
            }
        }
    }
    
    public override string GetDescription()
    {
        string plural = quantity > 1 ? "s" : "";
        string desc = string.IsNullOrEmpty(objectDescription) ? "" : $"that {objectDescription}";
        return $"Spawn {quantity} {objectName}{plural} {desc}";
    }
    
    private IEnumerator ReenableCollider(Collider2D col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null) col.enabled = true;
    }
}
