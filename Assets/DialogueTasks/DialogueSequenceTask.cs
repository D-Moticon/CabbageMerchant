using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueSequenceTask : DialogueTask
{
    [SerializeReference]
    public List<DialogueTask> dialogueTasks;
    public override IEnumerator RunTask(DialogueContext dc)
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
