using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeReference]
    public List<DialogueTask> dialogueTasks;
    
    public IEnumerator PlayDialogue(DialogueContext dc)
    {
        yield return new WaitForSeconds(0.5f); //allow scene to slide in
        dc.dialogueBox.HideAllChoiceButtons();
        
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