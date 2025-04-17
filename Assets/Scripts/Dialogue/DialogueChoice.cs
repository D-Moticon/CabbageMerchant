using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class DialogueChoice
{
    public string choiceButtonLabel;
    [SerializeReference]
    public List<DialogueTask> dialogueTasks;
    /*[SerializeReference]
    public List<DialogueCondition> conditions;*/
    [HideInInspector]public DialogueButton dialogueButton;

    public void TryDialogueChoice(DialogueContext dc)
    {
        /*for (int i = 0; i < conditions.Count; i++)
        {
            if (!conditions[i].IsConditionMet())
            {
                return;
            }
        }*/
    }

    public IEnumerator RunChoiceBranch(DialogueContext dc)
    {
        for (int i = 0; i < dialogueTasks.Count; i++)
        {
            Task t = new Task(dialogueTasks[i].RunTask(dc));
            while (t.Running)
            {
                yield return null;
            }
        }
    }
}
