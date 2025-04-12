using System;
using UnityEngine;
using TMPro;

public class RoundScoreUI : MonoBehaviour
{
    public TMP_Text roundScoreText;
    public TMP_Text roundGoalText;

    private void OnEnable()
    {
        GameStateMachine.RoundGoalUpdatedEvent += RoundGoalUpdated;
        GameStateMachine.RoundScoreUpdatedEvent += RoundScoreUpdated;
    }

    private void OnDisable()
    {
        GameStateMachine.RoundGoalUpdatedEvent -= RoundGoalUpdated;
        GameStateMachine.RoundScoreUpdatedEvent -= RoundScoreUpdated;
    }

    void RoundGoalUpdated(double newGoal)
    {
        roundGoalText.text = Helpers.FormatWithSuffix(newGoal);
    }

    void RoundScoreUpdated(double newScore)
    {
        roundScoreText.text = Helpers.FormatWithSuffix(newScore);
    }
}
