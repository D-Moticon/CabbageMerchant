using System;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class Fire : MonoBehaviour
{
    public PooledObjectData thisPooledObject;
    [FormerlySerializedAs("bonksPerSecond")] public float bonksPerSecondBase = 1f;
    private static float bonksPerSecondPower = .5f;
    private float secondsPerBonk = 1f;
    public float bonkValue = 1f;
    public int stacksRemaining = 0;
    public TMP_Text stackText;
    public MMF_Player stackFeel;
    public MMF_Player bonkFeel;
    public PooledObjectData bonkVFX;
    public SFXInfo bonkSFX;
    public PooledObjectData killVFX;
    public Sprite spriteForFloater;
    public Color floaterColor;
    public SpriteRenderer spriteRenderer;

    public static int stacksToGiveBallsOnHit = 1;
    
    private IBonkable bonkable;
    private float stackTimer = 0;

    private bool inBounceState = false;

    private void OnEnable()
    {
        GameStateMachine.EnteringBounceStateAction += OnBounceStateEntered;
        GameStateMachine.ExitingBounceStateAction += OnBounceStateExited;
        Ball.BallHitBonkableEvent += OnBallBonkedBonkable;
        Cabbage.CabbageMergedEventPreDestroy += CabbageMergedListener;
        Cabbage.CabbageRemovedEvent += CabbageRemovedListener;
        CalculateSecondsPerBonk();
        stackTimer = secondsPerBonk;

        if (spriteRenderer != null)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_Hue", (Singleton.Instance.playerStats.flameLevel-1)/2.5f);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringBounceStateAction -= OnBounceStateEntered;
        GameStateMachine.ExitingBounceStateAction -= OnBounceStateExited;
        Ball.BallHitBonkableEvent -= OnBallBonkedBonkable;
        Cabbage.CabbageMergedEventPreDestroy -= CabbageMergedListener;
        Cabbage.CabbageRemovedEvent -= CabbageRemovedListener;
    }

    private void Update()
    {
        if (!inBounceState)
        {
            return;
        }

        if (Singleton.Instance.pauseManager.isPaused)
        {
            return;
        }
        
        stackTimer -= Time.deltaTime;
        if (stackTimer <= 0)
        {
            PopStack();
            CalculateSecondsPerBonk();
            stackTimer = secondsPerBonk;
            
            if (stacksRemaining <= 0)
            {
                StopFire();
            }
        }
    }

    void CalculateSecondsPerBonk()
    {
        if (stacksRemaining <= 0)
        {
            secondsPerBonk = 1f / bonksPerSecondBase;
            return;
        }
        secondsPerBonk = 1f / (bonksPerSecondBase*Mathf.Pow(stacksRemaining,bonksPerSecondPower));
    }

    void PopStack()
    {
        float finalBonkValue = bonkValue * Singleton.Instance.playerStats.flameLevel;
        
        BonkParams bp = new BonkParams();
        bp.bonkerPower = finalBonkValue;
        bp.collisionPos = this.transform.position;
        bp.normal = Vector2.up;
        bp.overrideSFX = true;
        bonkable.Bonk(bp);
        
        stacksRemaining--;
        
        bonkVFX.Spawn(this.transform.position);
        bonkSFX.Play();
        bonkFeel.PlayFeedbacks();
        stackText.text = $"{(stacksRemaining*finalBonkValue):F0}";
        Singleton.Instance.floaterManager.SpawnSpriteFloater(finalBonkValue.ToString(), this.transform.position, spriteForFloater, floaterColor);
    }

    void StopFire()
    {
        if (killVFX != null)
        {
            killVFX.Spawn(this.transform.position);
        }
        gameObject.SetActive(false);
        if (GameSingleton.Instance != null)
        {
            GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
        }
    }

    public void AddStacks(int stacks)
    {
        stacksRemaining += stacks;
        stackText.text = stacksRemaining.ToString();
        stackFeel.PlayFeedbacks();
    }
    
    public static Fire SetBonkableOnFire(IBonkable b, int stacks)
    {
        Fire existingFire = b.GetGameObject().GetComponentInChildren<Fire>();
        if (existingFire != null)
        {
            existingFire.gameObject.SetActive(true);
            existingFire.AddStacks(stacks);
            return existingFire;
        }
        
        Fire fire = Singleton.Instance.prefabReferences.firePrefab.Spawn(b.GetGameObject().transform.position).GetComponent<Fire>();
        fire.transform.parent = b.GetGameObject().transform;
        fire.transform.localScale = Vector3.one;
        fire.SetStackNumber(stacks);
        fire.bonkable = b;
        fire.inBounceState = true;
        return fire;
    }

    void SetStackNumber(int newStacks)
    {
        stacksRemaining = newStacks;
        stackText.text = newStacks.ToString();
        stackFeel.PlayFeedbacks();
    }

    void OnBounceStateEntered()
    {
        inBounceState = true;
    }

    void OnBounceStateExited()
    {
        inBounceState = false;
    }

    void OnBallBonkedBonkable(Ball.BallHitBonkableParams bcParams)
    {
        if (bcParams.bonkable != bonkable)
        {
            return;
        }

        if (bcParams.ball == null)
        {
            return;
        }
        
        BallFire.SetBallOnFire(bcParams.ball, stacksToGiveBallsOnHit);
    }

    void CabbageMergedListener(Cabbage.CabbageMergedParams cmp)
    {
        if (cmp.oldCabbageA as Object == bonkable as Object || cmp.oldCabbageB as Object == bonkable as Object)
        {
            SetBonkableOnFire(cmp.newCabbage, stacksRemaining);
            StopFire();
        }
    }
    
    private void CabbageRemovedListener(Cabbage c)
    {
        if (c.gameObject == bonkable.GetGameObject())
        {
            StopFire();
        }
    }
}
