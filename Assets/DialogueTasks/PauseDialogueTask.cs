using System.Collections;
using UnityEngine;

public class PauseDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.pauseManager.SetPaused(true);
        yield break;
    }
}
