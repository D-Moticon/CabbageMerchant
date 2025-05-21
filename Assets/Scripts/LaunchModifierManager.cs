using System;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class LaunchModifierManager : MonoBehaviour
{
    //Food effects
    public bool forceNextBallRainbow = false;
    public float forceNextBallBonkValue = 1f;
    public float forceNextBallScale = 1f;
    public PhysicsMaterial2D forceNextBallPhysMat = null;
    public int forceNextBallFireStacks = 0;

    public GameObject nextBallScaleIcon;
    public TMP_Text nextBallScaleText;
    public GameObject nextBallBonkValueIcon;
    public TMP_Text nextBallBonkValueText;
    public GameObject nextBallFireIcon;
    public TMP_Text nextBallFireText;

    private void OnEnable()
    {
        GameStateMachine.BallFiredEarlyEvent += OnBallFiredEarly;
        RunManager.RunEndedEvent += RunEndedListener;
        ResetAllModifiers();
    }

    private void OnDisable()
    {
        GameStateMachine.BallFiredEarlyEvent -= OnBallFiredEarly;
        RunManager.RunEndedEvent -= RunEndedListener;
    }

    void RunEndedListener()
    {
        ResetAllModifiers();
    }

    void OnBallFiredEarly(Ball ball)
    {
        if (ball == null)
        {
            return;
        }

        if (GameSingleton.Instance == null)
        {
            //prevent golf balls from being modded
            return;
        }
        
        if (forceNextBallPhysMat != null)
        {
            ball.rb.sharedMaterial = Singleton.Instance.launchModifierManager.forceNextBallPhysMat;
            ball.col.sharedMaterial = Singleton.Instance.launchModifierManager.forceNextBallPhysMat;
        }
        
        float sca = forceNextBallScale;
        if (sca > 1.01f)
        {
            ball.SetScale(sca);
        }

        if (forceNextBallFireStacks > 0)
        {
            BallFire.SetBallOnFire(ball, forceNextBallFireStacks);
        }
        
        ResetAllModifiers();
    }

    void ResetAllModifiers()
    {
        forceNextBallRainbow = false;
        forceNextBallBonkValue = 1f;
        nextBallBonkValueIcon.SetActive(false);
        forceNextBallScale = 1f;
        forceNextBallPhysMat = null;
        nextBallScaleIcon.SetActive(false);
        forceNextBallFireStacks = 0;
        nextBallFireIcon.SetActive(false);
    }
    
    public void AddNextBallValue(float bonkValueAdd)
    {
        forceNextBallRainbow = true;
        forceNextBallBonkValue += bonkValueAdd;
        nextBallBonkValueIcon.SetActive(true);
        nextBallBonkValueText.text = $"{forceNextBallBonkValue}x";
        nextBallBonkValueIcon.GetComponentInChildren<MoreMountains.Feedbacks.MMF_Player>().PlayFeedbacks();
    }

    public void IncreaseNextBallScale(float scaleAdd)
    {
        forceNextBallScale += scaleAdd;
        nextBallScaleIcon.gameObject.SetActive(true);
        nextBallScaleText.text = $"{forceNextBallScale}x";
        nextBallScaleIcon.GetComponentInChildren<MoreMountains.Feedbacks.MMF_Player>().PlayFeedbacks();
    }

    public void ForceNextBallPhysMat(PhysicsMaterial2D physMat)
    {
        forceNextBallPhysMat = physMat;
    }

    public void ForceNextBallFire(int stacksToAdd)
    {
        forceNextBallFireStacks += stacksToAdd;
        nextBallFireIcon.gameObject.SetActive(true);
        nextBallFireText.text = forceNextBallFireStacks.ToString();
        nextBallFireIcon.GetComponentInChildren<MoreMountains.Feedbacks.MMF_Player>().PlayFeedbacks();
    }
}
