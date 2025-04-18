using System.Collections;
using UnityEngine;

public class AddCoinsDialogueTask : DialogueTask
{
    public int coinAmount;
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Singleton.Instance.playerStats.AddCoins(coinAmount);
        Singleton.Instance.itemManager.sellSFX.Play();
        Singleton.Instance.itemManager.sellVFX.transform.position =
            Singleton.Instance.uiManager.coinsText.transform.position;
        Singleton.Instance.itemManager.sellVFX.Play();

        yield return new WaitForSeconds(1f);
    }
}
