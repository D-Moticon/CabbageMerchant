using System;
using UnityEngine;

public class DestroyOnRoundComplete : MonoBehaviour
{
    private void OnEnable()
    {
        GameStateMachine.ClearBoardAction += ClearBoardListener;
    }
    
    private void OnDisable()
    {
        GameStateMachine.ClearBoardAction -= ClearBoardListener;
    }
    
    private void ClearBoardListener()
    {
        Destroy(gameObject);
    }
}
