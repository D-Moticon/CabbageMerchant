using System;
using UnityEngine;

public class DeactivateAfterRound : MonoBehaviour
{
    private void OnEnable()
    {
        GameStateMachine.ExitingBounceStateAction += BounceStateExitedListener;
    }

    private void OnDisable()
    {
        GameStateMachine.ExitingBounceStateAction -= BounceStateExitedListener;
    }

    void BounceStateExitedListener()
    {
        gameObject.SetActive(false);
    }
}
