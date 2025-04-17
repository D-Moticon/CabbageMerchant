using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

[System.Serializable]
public class DialogueLine : DialogueTask
{
    public DialogueCharacter dialogueCharacter;
    [TextAreaAttribute]public string dialogueLine;
    [FoldoutGroup("Extras")] public bool hideCharacterName = false;
    [FoldoutGroup("Extras")] public bool waitForSkipInput = true;
    [FoldoutGroup("Extras")] public float extraWaitTime = 0f;
    [FoldoutGroup("Extras")] public bool centerText = false;
    [FoldoutGroup("Extras")] public bool playSFX = true;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        if (centerText)
        {
            dc.dialogueBox.dialogueTextAnimator.TMProComponent.alignment = TextAlignmentOptions.Center;
        }

        else
        {
            dc.dialogueBox.dialogueTextAnimator.TMProComponent.alignment = TextAlignmentOptions.TopLeft;
        }
        
        
        if (dialogueCharacter != null)
        {
            dc.dialogueBox.characterImage.enabled = true;
            dc.dialogueBox.SetCharacterImage(dialogueCharacter.sprite);
        }
        else
        {
            dc.dialogueBox.characterImage.enabled = false;
        }

        if (dialogueCharacter == null || hideCharacterName)
        {
            dc.dialogueBox.nameTextAnimator.SetText("");
        }

        else
        {
            dc.dialogueBox.nameTextAnimator.SetText(dialogueCharacter.displayName);
        }
        
        if (dc.dialogueBox.isHidden)
        {
            dc.dialogueBox.dialogueTextAnimator.SetText("");
            Task t = new Task(dc.dialogueBox.FadeInDialogueBox(1f));
            while (t.Running)
            {
                yield return null;
            }
        }
        
        dc.dialogueBox.dialogueTypewriter.ShowText(dialogueLine);
        
        if (dialogueCharacter != null && playSFX)
        {
            dc.dialogueBox.characterTalkPlayer.PlayFeedbacks();
            if (dialogueLine.Length < 20)
            {
                dialogueCharacter.speakingSFX_Short.Play();
            }
            
            else if (dialogueLine.Length < 40)
            {
                dialogueCharacter.speakingSFX_Med.Play();
            }
            
            else
            {
                dialogueCharacter.speakingSFX_Long.Play();
            }
        }
        
        if (waitForSkipInput)
        {
            while (!Singleton.Instance.playerInputManager.dialogueSkipDown)
            {
                yield return null;
            }
        }
        
        if (extraWaitTime > 0.001f)
        {
            yield return new WaitForSeconds(extraWaitTime);
        }
    }
}
