using UnityEngine;
using System.Collections;

public class MoveBoardTokenDialogueTask : DialogueTask
{
    public int steps;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        /*bool finished = false;
        Task t = new Task(GameSingleton.Instance.boardGameManager.MoveToken(steps));
        while (t.Running)*/
            yield return null;
    }
}
