using System.Collections;
using UnityEngine;

public class HideDialogueBoxTask : DialogueTask
{
    public float fadeDuration = 0.5f;
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Task t = new Task(dc.dialogueBox.FadeOutDialogueBox(fadeDuration));
        while (t.Running)
        {
            yield return null;
        }
    }
}
