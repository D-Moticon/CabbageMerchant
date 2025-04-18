using System;
using UnityEngine;
using System.Collections;

public class FadeCharacterTask : DialogueTask
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
        float oldAlpha = dc.dialogueBox.characterImage.color.a;
        float newAlpha = 0f;
        
        switch (fadeType)
        {
            case FadeType.fadeOut:
                newAlpha = 0f;
                break;
            case FadeType.fadeIn:
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
            dc.dialogueBox.characterImage.color = new Color(
                dc.dialogueBox.characterImage.color.r,
                dc.dialogueBox.characterImage.color.g,
                dc.dialogueBox.characterImage.color.b,
                alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dc.dialogueBox.characterImage.color = new Color(
            dc.dialogueBox.characterImage.color.r,
            dc.dialogueBox.characterImage.color.g,
            dc.dialogueBox.characterImage.color.b,
            newAlpha);
    }
}
