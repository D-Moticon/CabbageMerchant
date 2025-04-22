using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BranchDialogueTask : DialogueTask
{
    [SerializeReference]
    public List<DialogueCondition> conditions;

    [SerializeReference]
    public DialogueTask trueTask;
    [SerializeReference]
    public DialogueTask falseTask;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        DialogueTask taskToRun;
        
        bool anyConditionFailed = false;
        for (int i = 0; i < conditions.Count; i++)
        {
            if (!conditions[i].IsConditionMet())
            {
                anyConditionFailed = true;
                break;
            }
        }

        if (anyConditionFailed)
        {
            taskToRun = falseTask;
        }

        else
        {
            taskToRun = trueTask;
        }

        Task t = new Task(taskToRun.RunTask(dc));
        while (t.Running)
        {
            yield return null;
        }
    }
}
