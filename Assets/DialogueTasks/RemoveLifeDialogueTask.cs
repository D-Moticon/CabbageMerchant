using System.Collections;
using UnityEngine;

public class RemoveLifeDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.playerStats.RemoveLife();
        yield return new WaitForSeconds(1f);
    }
}
