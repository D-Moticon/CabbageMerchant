using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using System.Collections;

public class RoundScoreUI : MonoBehaviour
{
    public TMP_Text roundScoreText;
    public MMF_Player roundScoreFeel;
    public TMP_Text roundGoalText;
    public Slider roundScoreSlider;
    public TMP_Text roundScoreMultText;
    public MMF_Player roundGoalHitFeel;
    public SFXInfo roundGoalHitSFX;
    public PooledObjectData roundGoalHitVFX;
    public Transform roundGoalHitVFXLocation;
    private bool isFlashing = false; //Give the bar a second to flash before it drops to zero
    public float flashDuration = 0.5f;
    private float flashCounter = 0f;
    private double oldScore = 0;

    private void OnEnable()
    {
        GameStateMachine.RoundGoalUpdatedEvent += RoundGoalUpdated;
        GameStateMachine.RoundScoreUpdatedEvent += RoundScoreUpdated;
        GameStateMachine.BoardFinishedPopulatingAction += BoardPopulatedListener;
        GameStateMachine.RoundGoalOverHitEvent += RoundGoalHitListener;

    }

    private void OnDisable()
    {
        GameStateMachine.RoundGoalUpdatedEvent -= RoundGoalUpdated;
        GameStateMachine.RoundScoreUpdatedEvent -= RoundScoreUpdated;
        GameStateMachine.BoardFinishedPopulatingAction -= BoardPopulatedListener;
        GameStateMachine.RoundGoalOverHitEvent -= RoundGoalHitListener;
    }

    void RoundGoalUpdated(double newGoal)
    {
        roundGoalText.text = Helpers.FormatWithSuffix(newGoal);
    }

    void RoundScoreUpdated(double newScore)
    {
        if (GameSingleton.Instance.gameStateMachine.roundGoal < 1)
        {
            roundScoreMultText.text = $"0<size=5>x";
            roundScoreSlider.value = 0;
        }
        roundScoreText.text = Helpers.FormatWithSuffix(newScore);
        double roundScoreMult = GameSingleton.Instance.gameStateMachine.currentRoundScoreOverMult;

        if (double.IsNaN(roundScoreMult))
        {
            roundScoreMultText.text = $"0<size=5>x";
            roundScoreSlider.value = 0;
            return;
        }

        if (newScore > oldScore)
        {
            roundScoreFeel.PlayFeedbacks();
        }
        
        double sliderVal = roundScoreMult % 1;
        
        //Give a slight delay before jumping back down to zero so user can see the flash
        if (isFlashing)
        {
            flashCounter -= Time.deltaTime;
            if (flashCounter <= 0f)
            {
                isFlashing = false;
            }
            sliderVal = 1;
        }
        
        roundScoreSlider.value = (float)sliderVal;
        roundScoreMultText.text = $"{Math.Floor(roundScoreMult).ToString()}<size=5>x";

        oldScore = newScore;
    }

    void BoardPopulatedListener()
    {
        roundScoreSlider.value = 0;
        roundScoreMultText.text = "";
    }

    void RoundGoalHitListener(double multiple)
    {
        roundGoalHitFeel.PlayFeedbacks();
        roundGoalHitVFX.Spawn(roundGoalHitVFXLocation.position);
        roundGoalHitSFX.Play();
        isFlashing = true;
        flashCounter = flashDuration;
    }

}
