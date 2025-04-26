using System.Collections;
using UnityEngine;

public class WaitForSecondsTask : DialogueTask
{
    public float duration = 1f;
    public override IEnumerator RunTask(DialogueContext dc)
    {
        yield return new WaitForSeconds(duration);
    }
}
