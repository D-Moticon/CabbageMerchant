using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DialogueTask_PopulateBoardGameBuckets : DialogueTask
{
    public List<int> moveAmounts = new List<int> { 1, 2, 3, 4, 5, 6 };

    public override IEnumerator RunTask(DialogueContext dc)
    {
        Task t = new Task(GameSingleton.Instance.boardGameManager.SpawnBuckets(moveAmounts));
        while (t.Running)
        {
            yield return null;
        }
        yield break;
    }
}
