using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ChoiceDialogueTask : DialogueTask
{
    [SerializeReference]
    public List<DialogueChoice> choices;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        dc.dialogueBox.ActivateButtons(choices.Count);
        for (int i = 0; i < choices.Count; i++)
        {
            choices[i].dialogueButton = dc.dialogueBox.choiceButtons[i];
            dc.dialogueBox.choiceButtons[i].buttonPressed = false;
            dc.dialogueBox.choiceButtons[i].SetText(choices[i].choiceButtonLabel);
        }

        IEnumerator taskToRun = null;
        
        while (taskToRun == null)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].dialogueButton.buttonPressed)
                {
                    taskToRun = choices[i].RunChoiceBranch(dc);
                    break;
                }
            }

            if (taskToRun == null)
            {
                yield return null;
            }
        }
        
        dc.dialogueBox.HideAllChoiceButtons();

        Task t = new Task(taskToRun);
        while (t.Running)
        {
            yield return null;
        }
    }
}
