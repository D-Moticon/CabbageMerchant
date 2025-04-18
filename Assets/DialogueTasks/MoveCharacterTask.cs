using System.Collections;
using UnityEngine;

public class MoveCharacterTask : DialogueTask
{
    public Vector2 newLocalPos = new Vector2(-2.3f, 0f);
    private float duration = 1f;
    AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Vector2 oldPos = dc.dialogueBox.characterImage.transform.localPosition;
        Vector2 newPos = newLocalPos;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float p = easeCurve.Evaluate(t);
            Vector2 pos = Vector2.Lerp(oldPos, newPos, p);
            dc.dialogueBox.characterImage.transform.localPosition = pos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dc.dialogueBox.characterImage.transform.localPosition = newPos;
    }
}
