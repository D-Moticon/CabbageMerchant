using System.Collections;
using UnityEngine;

public class AddMetacurrencyTask : DialogueTask
{
    public int metacurrencyEasy = 10;
    public int metacurrencyMedium = 20;
    public int metacurrencyHard = 30;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        int finalMetacurrency = 0;
        switch (Singleton.Instance.playerStats.currentDifficulty.difficultyLevel)
        {
            case 0:
                finalMetacurrency = metacurrencyEasy;
                break;
            case 1:
                finalMetacurrency = metacurrencyMedium;
                break;
            case 2:
                finalMetacurrency = metacurrencyHard;
                break;
            default:
                finalMetacurrency = metacurrencyHard;
                break;
        }
        
        Singleton.Instance.playerStats.AddMetacurrency(finalMetacurrency);
        Singleton.Instance.uiManager.DisplayMetacurrencyGainedAnimation(finalMetacurrency);
        yield return new WaitForSeconds(1f);
    }
}
