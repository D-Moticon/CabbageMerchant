using System.Collections;
using UnityEngine;

public class SpinWheelDialogueTask : DialogueTask
{
    public WheelSpinner wheelSpinnerPrefab;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        WheelSpinner wheelSpinner = GameObject.Instantiate(wheelSpinnerPrefab,
            dc.dialogueBox.itemSlotParent.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        
        Task spinTask = new Task(wheelSpinner.SpinRoutine(dc));
        while (spinTask.Running)
        {
            yield return null;
        }
    }
}
