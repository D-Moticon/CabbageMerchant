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

    public GameObject nextBallScaleIcon;
    public TMP_Text nextBallScaleText;
    public GameObject nextBallBonkValueIcon;
    public TMP_Text nextBallBonkValueText;
    

    private void OnEnable()
    {
        GameStateMachine.BallFiredEarlyEvent += OnBallFiredEarly;
        ResetAllModifiers();
    }

    private void OnDisable()
    {
        GameStateMachine.BallFiredEarlyEvent -= OnBallFiredEarly;
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
            ball.transform.localScale = new Vector3(sca, sca, 1f);
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
}
