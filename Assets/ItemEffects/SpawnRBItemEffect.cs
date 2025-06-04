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
    public Vector2 worldVelocityDirection = new Vector2(0f,1f);

    public bool makeRainbowBonker = false;
    public float rainbowConvertChance = 0.05f;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // Use the trigger context values:
        Vector2 pos = Vector2.zero;
        Vector2 normal = Vector2.up;

        switch (velocityDirection)
        {
            case VelocityDirection.Normal:
                if (tc != null && tc.normal != null)
                {
                    normal = tc.normal;
                }
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
                    if (tc != null && tc.ball != null)
                    {
                        if (tc.point != null)
                        {
                            pos = tc.point;
                        }
                        else
                        {
                            pos = tc.ball.transform.position;
                        }
                    }
                    else
                    {
                        Ball b = GameSingleton.Instance.gameStateMachine.GetRandomActiveBall();
                        if (b != null)
                        {
                            pos = b.transform.position;
                        }
                        else
                        {
                            BonkableSlot bs = GameSingleton.Instance.gameStateMachine.GetEmptyBonkableSlot();
                            if (bs != null)
                            {
                                pos = GameSingleton.Instance.gameStateMachine.GetEmptyBonkableSlot().transform.position;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    break;
                case SpawnLocation.worldArea:
                    pos = spawnCenter - spawnAreaSize * 0.5f + new Vector2(Random.Range(0f, spawnAreaSize.x), Random.Range(0f, spawnAreaSize.y));
                    break;
                default:
                    break;
            }
            
            float scaRand = Random.Range(scaleRange.x, scaleRange.y);
            
            GameObject spawnedGO = objectToSpawn.Spawn(pos + normal * surfaceOffset);
            if (spawnedGO == null)
            {
                Debug.LogError($"[SpawnRBItemEffect] objectToSpawn.Spawn(...) returned null for {objectToSpawn.name}");
                continue;
            }

            Rigidbody2D rb = spawnedGO.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"[SpawnRBItemEffect] The prefab '{spawnedGO.name}' has no Rigidbody2D component on it.");
                spawnedGO.SetActive(false);
                continue;
            }
            
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

            if (makeRainbowBonker)
            {
                Bonker bonker = rb.GetComponent<Bonker>();
                if (bonker != null)
                {
                    bonker.SetBonkerRainbow(rainbowConvertChance);
                }

                AddBonkMultOnCollide bonkMulter = rb.GetComponent<AddBonkMultOnCollide>();
                if (bonkMulter != null)
                {
                    bonkMulter.SetRainbow();
                }
            }

            else
            {
                AddBonkMultOnCollide bonkMulter = rb.GetComponent<AddBonkMultOnCollide>();
                if (bonkMulter != null)
                {
                    bonkMulter.SetNormal();
                }
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
        string rainbow = "";
        string rainbowSuffix = "";
        if (makeRainbowBonker)
        {
            rainbow = " <rainb>Rainbow</rainb>";
            rainbowSuffix =
                $"and have a {Helpers.ToPercentageString(rainbowConvertChance)} chance to turn cabbage <rainb>rainbow</rainb>";
        }
        string desc = string.IsNullOrEmpty(objectDescription) ? "" : $"that {objectDescription}";
        return $"Spawn {quantity}{rainbow} {objectName}{plural} {desc} {rainbowSuffix}";
    }
    
    private IEnumerator ReenableCollider(Collider2D col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null) col.enabled = true;
    }
    
    public override void RandomizePower()
    {
        base.RandomizePower();
        quantity = Random.Range(1, 40);
    }
}
