using System.Collections;
using UnityEngine;

public class GiveCoinsIfBelowThreshold : DialogueTask
{
    public int coinThreshold = 10;
    public override IEnumerator RunTask(DialogueContext dc)
    {
        if (Singleton.Instance.playerStats.coins < coinThreshold)
        {
            Singleton.Instance.playerStats.AddCoins(coinThreshold-Singleton.Instance.playerStats.coins);
        }
        
        yield break;
    }
}
