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
    [SerializeReference]
    public List<DialogueCondition> conditions;
    public int cost = 0;
    public DialogueLine conditionsNotMetLine;

    public bool AreConditionsMet(DialogueContext dc)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (!conditions[i].IsConditionMet())
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator RunChoiceBranch(DialogueContext dc)
    {
        if (cost > 0)
        {
            Singleton.Instance.playerStats.AddCoins(-cost);
        }
        
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
