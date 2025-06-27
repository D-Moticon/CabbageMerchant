using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GenerateBoardGameDialogueTask : DialogueTask
{
    public List<BoardGameManager.SquareInfo> availableSquares;
    public int boardLength = 12;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        GameSingleton.Instance.boardGameManager.boardGameActive = true;
        GameSingleton.Instance.boardGameManager.gameObject.SetActive(true);
        GameSingleton.Instance.boardGameManager.boardGameActive = true;
        Task t = new Task(GameSingleton.Instance.boardGameManager.GenerateBoard(availableSquares, boardLength));
        while (t.Running)
        {
            yield return null;
        }
        yield break;
    }
}
