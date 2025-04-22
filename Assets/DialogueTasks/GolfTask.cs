using System.Collections;
using UnityEngine;

public class GolfTask : DialogueTask
{
    public GolfStateMachine golfGamePrefab;
    public DialogueLine explanationLine;
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Task explanationtask = new Task(explanationLine.RunTask(dc));
        while (explanationtask.Running) yield return null;
        
        dc.dialogueBox.HideDialogueBox();
        
        GolfStateMachine gsm = GameObject.Instantiate(golfGamePrefab, Vector3.zero, Quaternion.identity) as GolfStateMachine;
        Task t = new Task(gsm.GolfRoutine(dc));
        while (t.Running) yield return null;

        GameObject.Destroy(gsm.gameObject);
    }
}
