using System;
using UnityEngine;

public class StopTryButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        GameSingleton.Instance.gameStateMachine.StopTry();
    }
}
