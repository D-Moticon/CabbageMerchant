using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

public class BoardGameSquare : MonoBehaviour
{
    public SpriteRenderer iconSR;
    public SpriteRenderer frameSR;
    [HideInInspector]public BoardGameSquareSO squareData;
    public MMF_Player hopFeel;
    public SFXInfo hopSFX;

    public void SetSquareData(BoardGameSquareSO bso)
    {
        squareData = bso;
        if (iconSR != null)
        {
            if (bso.icon != null)
            {
                iconSR.enabled = true;
                iconSR.sprite = bso.icon;
            }

            else
            {
                iconSR.enabled = false;
            }
        }

        frameSR.color = bso.frameColor;
    }
    
    public IEnumerator RunOnLandedTasks()
    {
        if (squareData == null || squareData.onLandedTasks == null)
            yield break;
        
        DialogueContext dc = new DialogueContext
        {
            dialogueBox = Singleton.Instance.dialogueManager.dialogueBox
        };

        foreach (var task in squareData.onLandedTasks)
        {
            Task t = new Task(task.RunTask(dc));
            while (t.Running)
                yield return null;
        }
    }

    public void PlayHopFeel()
    {
        hopFeel.PlayFeedbacks();
        hopSFX.Play(this.transform.position);
    }
}
