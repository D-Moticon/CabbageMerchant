using System.Collections;
using UnityEngine;

public class FadeDialogueBoxTask : DialogueTask
{
    public enum FadeType
    {
        fadeOut,
        fadeIn
    }

    public FadeType fadeType;
    
    public float duration = 1f;
    AnimationCurve easeCurve = AnimationCurve.Linear(0f,0f,1f,1f);
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        float oldAlpha = dc.dialogueBox.canvasGroup.alpha;
        float newAlpha = 0f;
        
        switch (fadeType)
        {
            case FadeType.fadeOut:
                newAlpha = 0f;
                break;
            case FadeType.fadeIn:
                dc.dialogueBox.SetDialogueText("",false);
                dc.dialogueBox.SetCharacterNameText("");
                newAlpha = 1f;
                break;
            default:
                break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float p = easeCurve.Evaluate(t);
            float alpha = Mathf.Lerp(oldAlpha, newAlpha, p);
            dc.dialogueBox.canvasGroup.alpha = alpha;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dc.dialogueBox.canvasGroup.alpha = newAlpha;
    }
}
