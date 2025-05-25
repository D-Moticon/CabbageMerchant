using System;
using UnityEngine;
using TMPro;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;
    public Collider2D col;
    public SpriteRenderer sr;
    public TrailRenderer tr;
    public float baseBonkValue = 1f;
    public float bonkValue = 1f;
    public TMP_Text bonkValueText;
    public string bonkValueMaterialProp;
    public Vector2Int bonkValueRangeForMat = new Vector2Int(1, 10);
    public Vector2 bonkValueMatPropRange = new Vector2(0f, 1f);
    public SFXInfo bonkValueUpSFX;
    public Color bonkValueUpFloaterColor = Color.green;
    public PooledObjectData bonkValueUpVFX;
    public FloaterReference bonkValueUpFloater;
    public PooledObjectData pooledObjectRef;
    
    public class BallHitBonkableParams
    {
        public Ball ball;
        public Cabbage cabbage;
        public IBonkable bonkable;
        public Vector2 point;
        public Vector2 normal;
        public float hangTimeBeforeHit;
    }
    
    public delegate void BallCabbageDelegate(BallHitBonkableParams bcParams);
    public static BallCabbageDelegate BallHitBonkableEvent;

    public delegate void CollisionDelegate(Ball b, Collision2D col);
    public static event CollisionDelegate BallCollidedEvent;

    public delegate void BallDelegate(Ball b);

    public static BallDelegate BallEnabledEvent;
    public static BallDelegate BallDisabledEvent;

    public class BallSpawnParams
    {
        private Ball ball;
    }
    
    private static float timeoutDuration = 0.5f;
    private static float timeoutVel = 0.1f;
    private float timeoutCounter = 0f;
    public SFXInfo wallBonkSFX;
    public float bonkCooldown = 0.05f;
    private float bonkCooldownCounter;
    private float currentHangtime = 0f;
    
    private void OnEnable()
    {
        //GameSingleton.Instance.gameStateMachine.AddActiveBall(this);
        bonkValue = baseBonkValue;
        BallEnabledEvent?.Invoke(this);
        bonkValueText.text = Helpers.FormatWithSuffix(bonkValue);
        bonkValueText.enabled = false;
        SetBonkValueMatProp();
    }

    private void OnDisable()
    {
        BallDisabledEvent?.Invoke(this);
    }

    public void SetVelocity(Vector2 vel)
    {
        rb.linearVelocity = vel;
    }

    private void Update()
    {
        if (!Singleton.Instance.boundsManager.IsObjectInPlayBounds(this.gameObject))
        {
            KillBall();
        }

        if (rb.linearVelocity.magnitude < timeoutVel)
        {
            timeoutCounter += Time.deltaTime;
            if (timeoutCounter > timeoutDuration)
            {
                KillBall();
            }
        }

        else
        {
            timeoutCounter = 0f;
        }

        if (bonkCooldownCounter > 0f)
        {
            bonkCooldownCounter -= Time.deltaTime;
        }

        currentHangtime += Time.deltaTime;
    }

    public void KillBall()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        BallCollidedEvent?.Invoke(this, other);

        IBonkable b = other.gameObject.GetComponent<IBonkable>();
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (b != null && bonkCooldownCounter <= 0f)
        {
            BonkParams bp = new BonkParams();
            bp.bonkerPower = bonkValue;
            bp.collisionPos = other.GetContact(0).point;
            bp.normal = other.GetContact(0).normal;
            bp.ball = this;
            bp.treatAsBall = true;
            b.Bonk(bp);

            BallHitBonkableParams bcParams = new BallHitBonkableParams();
            bcParams.ball = this;
            bcParams.cabbage = c;
            bcParams.bonkable = b;
            bcParams.point = other.GetContact(0).point;
            bcParams.normal = other.GetContact(0).normal;
            bcParams.hangTimeBeforeHit = currentHangtime;
            
            BallHitBonkableEvent?.Invoke(bcParams);
            bonkCooldownCounter = bonkCooldown;
            currentHangtime = 0f;
        }

        else
        {
            wallBonkSFX.Play(other.GetContact(0).point);
        }
    }

    public void AddBonkValue(float bonkValueAdd)
    {
        bonkValue += bonkValueAdd;
        bonkValueText.text = Helpers.FormatWithSuffix(bonkValue);
        bonkValueText.enabled = true;
        SetBonkValueMatProp();
        bonkValueUpSFX.Play();
        bonkValueUpVFX.Spawn(this.transform.position);
        if (bonkValueAdd > 0.09f)
        {
            bonkValueUpFloater.Spawn($"+{bonkValueAdd:F1}", rb.position, bonkValueUpFloaterColor, 0.75f);
        }
    }

    void SetBonkValueMatProp()
    {
        float propValue = Helpers.RemapClamped(bonkValue, bonkValueRangeForMat.x, bonkValueRangeForMat.y,
            bonkValueMatPropRange.x, bonkValueMatPropRange.y);
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(mpb);
        mpb.SetFloat(bonkValueMaterialProp, propValue);
        sr.SetPropertyBlock(mpb);
        
        MaterialPropertyBlock tmpb = new MaterialPropertyBlock();
        tr.GetPropertyBlock(tmpb);
        tmpb.SetFloat(bonkValueMaterialProp, propValue);
        tr.SetPropertyBlock(tmpb);
    }

    public void DestroyBall()
    {
        gameObject.SetActive(false);
    }

    public void SetScale(float sca)
    {
        transform.localScale = new Vector3(sca, sca, 1f);
        tr.widthMultiplier = sca;
    }
}
