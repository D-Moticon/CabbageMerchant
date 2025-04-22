using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ChoiceDialogueTask : DialogueTask
{
    [SerializeReference]
    public List<DialogueChoice> choices;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        // 1) Activate and label all buttons once
        dc.dialogueBox.ActivateButtons(choices.Count);
        for (int i = 0; i < choices.Count; i++)
        {
            choices[i].dialogueButton = dc.dialogueBox.choiceButtons[i];
            choices[i].dialogueButton.buttonPressed = false;

            string costString = choices[i].cost > 0 
                ? $"\n({choices[i].cost} coins)" 
                : "";
            choices[i].dialogueButton.SetText(
                choices[i].choiceButtonLabel + costString);
        }

        // 2) Store the original dialogue text so we can restore it
        string originalText = dc.dialogueBox.dialogueText.text;

        // 3) Loop until we actually pick a valid branch
        IEnumerator taskToRun = null;
        while (taskToRun == null)
        {
            // scan for a pressed button
            int pressedIndex = -1;
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].dialogueButton.buttonPressed)
                {
                    pressedIndex = i;
                    break;
                }
            }

            if (pressedIndex < 0)
            {
                // nobody pressed yet, wait a frame
                yield return null;
                continue;
            }

            var choice = choices[pressedIndex];

            // -- CAN’T AFFORD branch --
            if (choice.cost > Singleton.Instance.playerStats.coins)
            {
                // show “can’t afford” line
                DialogueLine dl = new DialogueLine { dialogueLine = "You can't afford it!" };
                Task cantAfford = new Task(dl.RunTask(dc));
                while (cantAfford.Running) yield return null;

                // restore original text & reset that button so they can pick again
                dc.dialogueBox.dialogueText.text = originalText;
                choice.dialogueButton.buttonPressed = false;

                // jump back to top of while → re‐wait for input
                continue;
            }

            // -- CONDITIONS MET branch --
            if (choice.AreConditionsMet(dc))
            {
                taskToRun = choice.RunChoiceBranch(dc);
                break;
            }

            // -- CONDITIONS NOT MET branch --
            {
                Task notMet = new Task(choice.conditionsNotMetLine.RunTask(dc));
                while (notMet.Running) yield return null;

                // restore original text & reset so they can try again
                dc.dialogueBox.dialogueText.text = originalText;
                choice.dialogueButton.buttonPressed = false;

                continue;
            }
        }

        // 4) Once we have a valid task, hide buttons and run it
        dc.dialogueBox.HideAllChoiceButtons();
        Task branch = new Task(taskToRun);
        while (branch.Running) yield return null;
    }

}
