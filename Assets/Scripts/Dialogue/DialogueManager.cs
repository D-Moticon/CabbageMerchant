using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    private Queue<Task> dialogueTasks = new Queue<Task>();
    private Task currentTask;
    public DialogueBox eventDialogueBox;
    public Transform itemSlotParent;
    
    
    public void AddDialogueTask(Task task)
    {
        dialogueTasks.Enqueue(task);
    }

    public IEnumerator DialogueTaskRoutine(Task firstTask)
    {
        dialogueTasks.Clear();
        AddDialogueTask(firstTask);

        while (dialogueTasks.Count > 0)
        {   
            currentTask = dialogueTasks.Dequeue();
            if (currentTask == null)
            {
                break;
            }
            
            while (currentTask.Running)
            {
                yield return null;
            }
        }
    }
}
