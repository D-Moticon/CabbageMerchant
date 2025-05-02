using System;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;

public class Fire : MonoBehaviour
{
    public PooledObjectData thisPooledObject;
    public float bonksPerSecond = 1f;
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

    public static int stacksToGiveBallsOnHit = 1;
    
    private IBonkable bonkable;
    private float stackTimer = 0;

    private bool inBounceState = false;

    private void OnEnable()
    {
        GameStateMachine.EnteringBounceStateAction += OnBounceStateEntered;
        GameStateMachine.ExitingBounceStateAction += OnBounceStateExited;
        Ball.BallHitBonkableEvent += OnBallBonkedBonkable;
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
        secondsPerBonk = 1f / bonksPerSecond;
        stackTimer = secondsPerBonk;
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringBounceStateAction -= OnBounceStateEntered;
        GameStateMachine.ExitingBounceStateAction -= OnBounceStateExited;
        Ball.BallHitBonkableEvent -= OnBallBonkedBonkable;
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
    }

    private void Update()
    {
        if (!inBounceState)
        {
            return;
        }
        
        stackTimer -= Time.deltaTime;
        if (stackTimer <= 0)
        {
            PopStack();
            stackTimer = secondsPerBonk;
            
            if (stacksRemaining <= 0)
            {
                StopFire();
            }
        }
    }

    void PopStack()
    {
        BonkParams bp = new BonkParams();
        bp.bonkerPower = bonkValue;
        bp.collisionPos = this.transform.position;
        bp.normal = Vector2.up;
        bonkable.Bonk(bp);
        
        stacksRemaining--;
        
        bonkVFX.Spawn(this.transform.position);
        bonkSFX.Play();
        bonkFeel.PlayFeedbacks();
        stackText.text = stacksRemaining.ToString();
        Singleton.Instance.floaterManager.SpawnSpriteFloater(bonkValue.ToString(), this.transform.position, spriteForFloater, floaterColor);
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
        if (cmp.oldCabbageA == bonkable || cmp.oldCabbageB == bonkable)
        {
            SetBonkableOnFire(cmp.newCabbage, stacksRemaining);
            StopFire();
        }
    }
}
