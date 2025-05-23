using System.Collections;
using UnityEngine;

public class AddLifeDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.playerStats.AddLife(1);
        yield return new WaitForSeconds(1f);
    }
}
