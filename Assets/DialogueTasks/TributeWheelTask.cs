using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TributeWheelTask : DialogueTask
{
    public WheelSpinner wheelSpinnerPrefab;
    public List<int> positiveSliceIndexes;
    public float weightPerCoinAdded = 0.25f;
    public float wheelYOffset = 0f;
    public PooledObjectData payVFX;
    public SFXInfo paySFX;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        WheelSpinner wheelSpinner = GameObject.Instantiate(wheelSpinnerPrefab,
            dc.dialogueBox.itemSlotParent.transform.position + Vector3.up*wheelYOffset, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        
        dc.dialogueBox.ActivateButtons(2);
        var tributeButton = dc.dialogueBox.choiceButtons[0];
        var spinButton = dc.dialogueBox.choiceButtons[1];
        tributeButton.SetText("Tribute 1 Coin");
        spinButton.SetText("Spin");
        tributeButton.buttonPressed = false;
        spinButton.buttonPressed = false;

        while (!spinButton.buttonPressed)
        {
            if (tributeButton.buttonPressed)
            {
                tributeButton.buttonPressed = false;
                if (Singleton.Instance.playerStats.coins > 0)
                {
                    Singleton.Instance.playerStats.AddCoins(-1);
                    if (payVFX != null)
                    {
                        payVFX.Spawn(tributeButton.transform.position);
                    }
                    paySFX.Play();
                    for (int i = 0; i < positiveSliceIndexes.Count; i++)
                    {
                        wheelSpinner.AddWeightToSegment(positiveSliceIndexes[i], weightPerCoinAdded);
                    }
                }
            }

            yield return null;
        }
        
        dc.dialogueBox.HideAllChoiceButtons();
        
        Task spinTask = new Task(wheelSpinner.SpinRoutine(dc));
        while (spinTask.Running)
        {
            yield return null;
        }
    }
}
