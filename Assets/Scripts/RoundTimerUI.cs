using System;
using UnityEngine;
using TMPro;

public class RoundTimerUI : MonoBehaviour
{
    public GameObject timerObject;
    public TMP_Text timerText;
    private void OnEnable()
    {
        GameStateMachine.TimerUpdatedEvent += OnTimerUpdated;
        GameStateMachine.EnteringBounceStateAction += OnEnteringBounceState;
        GameStateMachine.ExitingBounceStateAction += OnExitingBounceState;
        timerObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameStateMachine.TimerUpdatedEvent -= OnTimerUpdated;
        GameStateMachine.EnteringBounceStateAction -= OnEnteringBounceState;
        GameStateMachine.ExitingBounceStateAction -= OnExitingBounceState;
    }

    void OnTimerUpdated(float newTime)
    {
        timerText.text = $"{newTime:F1}";
    }

    void OnEnteringBounceState()
    {
        if (GameSingleton.Instance.gameStateMachine.usingTimer)
        {
            timerObject.SetActive(true);
        }
    }

    void OnExitingBounceState()
    {
        timerObject.SetActive(false);
    }
    
}
