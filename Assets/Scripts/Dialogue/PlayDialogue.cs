using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayDialogue : MonoBehaviour
{
    public DialogueBox dialogueBox;
    public List<Dialogue> dialoguesToChooseFrom;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueBox.HideDialogueBox();
        DialogueContext dc = new DialogueContext();
        dc.dialogueBox = dialogueBox;
        Task t = new Task(PlayDialogueTask(dc));
    }

    IEnumerator PlayDialogueTask(DialogueContext dc)
    {
        int rand = Random.Range(0, dialoguesToChooseFrom.Count);
        Dialogue d = dialoguesToChooseFrom[rand];
        Task t = new Task(d.PlayDialogue(dc));
        while (t.Running)
        {
            yield return null;
        }
        Singleton.Instance.runManager.GoToMap();
    }
}
