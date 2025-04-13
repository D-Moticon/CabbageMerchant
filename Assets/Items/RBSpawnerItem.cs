using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RBSpawnerItem : Item
{
    public PooledObjectData objectToSpawn;
    public int quantity = 1;
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

    protected override void TriggerItem(TriggerContext tc = null)
    {
        base.TriggerItem();
        
        if (tc == null)
        {
            return;
        }

        Vector2 normal = tc.normal;
        Vector2 pos = tc.point;

        for (int i = 0; i < quantity; i++)
        {
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
}
