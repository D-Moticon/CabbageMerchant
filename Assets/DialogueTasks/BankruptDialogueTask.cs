using System.Collections;
using UnityEngine;

public class BankruptDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.playerStats.AddCoins(-Singleton.Instance.playerStats.coins);
        yield return new WaitForSeconds(1f);
    }
}
