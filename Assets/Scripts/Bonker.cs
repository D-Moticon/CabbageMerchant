using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Bonker : MonoBehaviour
{
    public float bonkValue = 1f;
    public bool killOnBonk;
    public bool killOnWallCollide = false;
    public int bonksBeforeKill = 0;
    private int bonkCounter = 0;
    public bool treatAsBall = false;
    public PooledObjectData vfxOnBonk;
    public PooledObjectData vfxOnKill;
    public SFXInfo sfxOnKill;

    [Header("Rainbow")]
    public Material baseMat;
    public Material rainbowMat;
    private bool isRainbow = false;
    private float rainbowConvertChance = 0.05f;

    private void OnEnable()
    {
        bonkCounter = 0;
    }

    private void OnDisable()
    {
        SetBonkerNormal();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (killOnWallCollide && other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Kill();
            return;
        }

        IBonkable b = other.gameObject.GetComponent<IBonkable>();
        if (b != null)
        {
            Vector2 hitPoint = other.GetContact(0).point;
            Vector2 hitNormal = other.GetContact(0).normal;
            
            Bonk(b, other.collider, hitPoint, hitNormal);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        IBonkable b = other.gameObject.GetComponent<IBonkable>();
        if (b != null)
        {
            Vector2 pos = (this.transform.position + other.transform.position) / 2f;
            Vector2 normal = (this.transform.position - other.transform.position).normalized;
            
            Bonk(b, other, pos, normal);
        }
    }
    
    void Bonk(IBonkable b, Collider2D other, Vector2 pos, Vector2 normal)
    {
        if (b != null)
        {
            BonkParams bp = new BonkParams();
            bp.bonkerPower = bonkValue;
            bp.collisionPos = pos;
            bp.normal = normal;
            bp.ball = null;
            bp.treatAsBall = treatAsBall;
            b.Bonk(bp);

            if (isRainbow)
            {
                Cabbage c = other.GetComponent<Cabbage>();
                if (c != null)
                {
                    float rand = Random.value;
                    if (rand < rainbowConvertChance)
                    {
                        c.SetVariant(CabbageVariantType.rainbow);
                    }
                }
            }
            
            if (killOnBonk)
            {
                if (bonkCounter >= bonksBeforeKill)
                {
                    Kill();
                }

                bonkCounter++;
            }
            
        }
    }

    void Kill()
    {
        if (vfxOnKill != null)
        {
            vfxOnKill.Spawn(this.transform.position);
        }

        sfxOnKill.Play(this.transform.position);
        
        gameObject.SetActive(false);
    }

    public void SetBonkerNormal()
    {
        if (baseMat != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            sr.material = baseMat;
        }
        
        isRainbow = false;
    }
    
    public void SetBonkerRainbow(float convertChance = 0.05f)
    {
        if (rainbowMat != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            sr.material = rainbowMat;
        }
        
        isRainbow = true;
        rainbowConvertChance = convertChance;
    }
}
