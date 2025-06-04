using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public DialogueBox dialogueBox;
    [HideInInspector] public Dialogue nextSpecificDialogue; //Set this before changing scene to Event_Specific and this dialogue will play
    
    private void Awake()
    {
        dialogueBox.HideDialogueBox();
    }

    public void PlayDialogue(Dialogue d, bool pause = false)
    {
        StartCoroutine(DialogueTaskRoutine(d, pause));
    }

    public IEnumerator DialogueTaskRoutine(Dialogue d, bool pause = false)
    {
        if (d == null)
        {
            print("Tried to play dialogue but dialogue was null");
            yield break;
        }

        if (pause)
        {
            Singleton.Instance.pauseManager.SetPaused(true);
        }
        
        dialogueBox.gameObject.SetActive(true);
        Singleton.Instance.pauseManager.SetPaused(true);
        DialogueContext dc = new DialogueContext();
        dc.dialogueBox = dialogueBox;

        Task t = new Task(d.PlayDialogue(dc));
        while (t.Running)
        {
            yield return null;
        }
        
        Singleton.Instance.pauseManager.SetPaused(false);
        dialogueBox.HideDialogueBox();
        yield return new WaitForSeconds(0.5f);
        /*Task fadeTask = new Task(dialogueBox.FadeOutDialogueBox(0.35f));
        while (fadeTask.Running)
        {
            yield return null;
        }*/
        dialogueBox.gameObject.SetActive(false);
        
        if (pause)
        {
            Singleton.Instance.pauseManager.SetPaused(false);
        }
    }
    
    public IEnumerator DialogueTaskRoutine(List<DialogueTask> dialogueTasks)
    {
        if (dialogueTasks == null)
        {
            print("Tried to play dialogue but dialogue was null");
            yield break;
        }
        
        dialogueBox.gameObject.SetActive(true);
        Singleton.Instance.pauseManager.SetPaused(true);
        DialogueContext dc = new DialogueContext();
        dc.dialogueBox = dialogueBox;
        dc.dialogueBox.HideAllChoiceButtons();

        foreach (var dt in dialogueTasks)
        {
            Task t = new Task(dt.RunTask(dc));
            while (t.Running)
            {
                yield return null;
            }
        }
        
        Singleton.Instance.pauseManager.SetPaused(false);
        dialogueBox.HideDialogueBox();
        yield return new WaitForSeconds(0.5f);
        /*Task fadeTask = new Task(dialogueBox.FadeOutDialogueBox(0.35f));
        while (fadeTask.Running)
        {
            yield return null;
        }*/
        dialogueBox.gameObject.SetActive(false);
        
    }
    
    public void SetNextSpecificDialogue(Dialogue d)
    {
        nextSpecificDialogue = d;
    }
}
