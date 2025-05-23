using System.Collections;
using UnityEngine;

public class BankruptDialogueTask : DialogueTask
{
    public override IEnumerator RunTask(DialogueContext dc)
    {
        int coinAmount = -(int)Singleton.Instance.playerStats.coins;
        Singleton.Instance.playerStats.AddCoins(coinAmount);
        Singleton.Instance.uiManager.DisplayCoinsGainedAnimation(coinAmount);
        yield return new WaitForSeconds(1f);
    }
}
