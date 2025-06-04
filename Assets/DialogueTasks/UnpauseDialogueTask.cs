using System.Collections;
using UnityEngine;

public class UnpauseDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.pauseManager.SetPaused(false);
        yield break;
    }
}
