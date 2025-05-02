using System;
using UnityEngine;
using TMPro;

public class BallFire : MonoBehaviour
{
    public TMP_Text stacksRemainingText;
    public PooledObjectData thisPooledObject;
    private Ball owningBall;
    public static int fireStacksGivenPerBonk = 3;
    private int stacksRemaining;

    private void OnEnable()
    {
        Ball.BallHitBonkableEvent += OnBonkableBonked;
    }

    private void OnDisable()
    {
        Ball.BallHitBonkableEvent -= OnBonkableBonked;
    }

    public static void SetBallOnFire(Ball b, int initialStacks)
    {
        BallFire existingBF = b.GetComponentInChildren<BallFire>();
        
        if (existingBF != null)
        {
            existingBF.gameObject.SetActive(true);
            existingBF.AddStacks(initialStacks);
            return;
        }
        
        BallFire bf = Singleton.Instance.prefabReferences.ballFirePrefab.Spawn(b.transform.position).GetComponent<BallFire>();
        bf.InitializeBallFire(b, initialStacks);
    }
    
    void InitializeBallFire(Ball ball, int initialStacks)
    {
        owningBall = ball;
        transform.parent = ball.transform;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        SetStacks(initialStacks);
    }

    void SetStacks(int newStacks)
    {
        stacksRemaining = newStacks;
        stacksRemainingText.text = newStacks.ToString();
    }
    
    void OnBonkableBonked(Ball.BallHitBonkableParams bcParams)
    {
        if (bcParams.ball != owningBall)
        {
            return;
        }

        Fire fire = bcParams.bonkable.GetGameObject().GetComponentInChildren<Fire>();
        if (fire != null)
        {
            fire.gameObject.SetActive(true);
            
            if (fire.stacksRemaining > stacksRemaining)
            {
                return;
            }

            else
            {
                int stacksToAdd = Mathf.Min(stacksRemaining, fireStacksGivenPerBonk);
                stacksRemaining -= stacksToAdd;
                stacksRemainingText.text = stacksRemaining.ToString();
                fire.AddStacks(stacksToAdd);
                return;
            }
        }
        
        int stacksToGive = Mathf.Min(fireStacksGivenPerBonk, stacksRemaining);
        Fire.SetBonkableOnFire(bcParams.bonkable, stacksToGive);
        stacksRemaining -= stacksToGive;
        stacksRemainingText.text = stacksRemaining.ToString();
        if (stacksToGive <= 0)
        {
            EndFire();
        }
    }

    void EndFire()
    {
        if (GameSingleton.Instance != null)
        {
            GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
        }

        else
        {
            Destroy(this.gameObject);
        }
    }

    public int GetStacksRemaining()
    {
        return stacksRemaining;
    }

    public void AddStacks(int stacksToAdd)
    {
        SetStacks(stacksRemaining+stacksToAdd);
    }
}
