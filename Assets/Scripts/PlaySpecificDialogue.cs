using System;
using UnityEngine;
using System.Collections;

public class PlaySpecificDialogue : MonoBehaviour
{
    public DialogueBox dialogueBox;

    private void Start()
    {
        PlayDialogue(Singleton.Instance.dialogueManager.nextSpecificDialogue);
    }

    public void PlayDialogue(Dialogue d)
    {
        DialogueContext dc = new DialogueContext { dialogueBox = dialogueBox };
        Task t = new Task(PlayDialogueTask(d, dc));
    }
    
    public IEnumerator PlayDialogueTask(Dialogue d, DialogueContext dc)
    {
        Task dialogueTask = new Task(d.PlayDialogue(dc));
        while (dialogueTask.Running)
            yield return null;

        Singleton.Instance.runManager.GoToMap();
    }
    
}
