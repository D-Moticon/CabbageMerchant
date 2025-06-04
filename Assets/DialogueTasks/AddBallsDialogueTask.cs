using System.Collections;
using UnityEngine;

public class AddBallsDialogueTask : DialogueTask
{
    public int ballsToAdd = 1;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        GameSingleton.Instance.gameStateMachine.AddExtraBall(ballsToAdd);
        yield break;
    }
}
