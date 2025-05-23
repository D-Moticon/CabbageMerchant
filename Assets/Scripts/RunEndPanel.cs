using System;
using UnityEngine;
using TMPro;
using Febucci.UI;

public class RunEndPanel : MenuPanel
{
    public TypewriterByCharacter runResultTypewriter;
    public TMP_Text totalBonksText;
    public TMP_Text totalBonkValueText;
    public TMP_Text totalTimeText;

    public string runSuccessfulText;
    public string runFailedText;
    
    private void OnEnable()
    {
        RunManager.RunFinishedEvent += RunFinishedListener;
    }

    private void OnDisable()
    {
        RunManager.RunFinishedEvent -= RunFinishedListener;
    }

    public void ShowRunStats()
    {
        PlayerStats pStats = Singleton.Instance.playerStats;
        totalBonksText.text = $"Cabbages Bonked: {Helpers.FormatWithSuffix(pStats.totalCabbagesBonkedThisRun)}";
        totalBonkValueText.text = $"Total Bonk Value: {Helpers.FormatWithSuffix(pStats.totalBonkValueThisRun)}";
        
        var ts = System.TimeSpan.FromSeconds(pStats.totalRunTime);
        totalTimeText.text = "Run Time: " + ts.ToString(@"mm\:ss\.f");
    }

    void RunFinishedListener(RunManager.RunCompleteParams rep)
    {
        if (!string.IsNullOrEmpty(rep.customEndString))
        {
            runResultTypewriter.ShowText(rep.customEndString);
        }

        else
        {
            if (rep.success)
            {
                runResultTypewriter.ShowText(runSuccessfulText);
            }

            else
            {
                runResultTypewriter.ShowText(runFailedText);
            }
        }
        
        ShowRunStats();
    }
}
