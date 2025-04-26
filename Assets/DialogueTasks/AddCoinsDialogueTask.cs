using System.Collections;
using UnityEngine;

public class AddCoinsDialogueTask : DialogueTask
{
    public int coinAmount;
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.playerStats.AddCoins(coinAmount);
        Singleton.Instance.uiManager.DisplayCoinsGainedAnimation(coinAmount);

        yield return new WaitForSeconds(1f);
    }
}
