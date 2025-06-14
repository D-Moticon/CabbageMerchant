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
    [FoldoutGroup("Extras")] public Sprite overrideSprite;
    [FoldoutGroup("Extras")] public bool forceTopLayer = false;

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

        if (overrideSprite != null)
        {
            dc.dialogueBox.characterImage.enabled = true;
            dc.dialogueBox.SetCharacterImage(overrideSprite);
        }
        else if (dialogueCharacter != null)
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


        if (dc.dialogueBox.canvas != null)
        {
            if (forceTopLayer)
            {
                dc.dialogueBox.canvas.sortingOrder = dc.dialogueBox.topCanvasLayer;
            }

            else
            {
                dc.dialogueBox.canvas.sortingOrder = dc.dialogueBox.normalCanvasLayer;
            }
        }
        
        if (dc.dialogueBox.isHidden)
        {
            dc.dialogueBox.dialogueTextAnimator.SetText("");
            Task t = new Task(dc.dialogueBox.FadeInDialogueBox(.75f));
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
        
        else if (overrideSprite != null && playSFX)
        {
            dc.dialogueBox.characterTalkPlayer.PlayFeedbacks();
        }
        
        if (waitForSkipInput)
        {
            // keep polling until either they press the normal skip key
            // or they press fireDown while NOT hovering over any collider
            while (true)
            {
                // 1) Normal “skip dialogue” key
                if (Singleton.Instance.playerInputManager.dialogueSkipDown)
                    break;

                // 2) Fire button pressed, but only if we’re *not* over any collider
                if (Singleton.Instance.playerInputManager.fireDown)
                {
                    // If the click is inside our dialogue panel's RectTransform, break
                    if (RectTransformUtility.RectangleContainsScreenPoint(
                            dc.dialogueBox.GetComponent<RectTransform>(), 
                            Input.mousePosition, 
                            Camera.main))
                    {
                        break;
                    }
                    
                    /*// turn screen coords into world coords
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    // 2D example—checks if there's any Collider2D under the cursor
                    bool hovering = Physics2D.OverlapPoint(worldPos) != null;

                    // if nothing was hit, we can treat fireDown as a skip
                    if (!hovering)
                        break;*/
                }

                yield return null;
            }
        }
        
        if (extraWaitTime > 0.001f)
        {
            yield return new WaitForSeconds(extraWaitTime);
        }
    }
}