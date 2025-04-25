using System;
using UnityEngine;
using TMPro;
using Febucci.UI;

public class RunEndPanel : MenuPanel
{
    public TypewriterByCharacter runResultTypewriter;
    public TMP_Text totalBonksText;

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

    public void ShowRunStats(RunStats stats)
    {
        totalBonksText.text = $"Cabbages Bonked: {Helpers.FormatWithSuffix(stats.totalBonks)}";
    }

    void RunFinishedListener(RunManager.RunCompleteParams rep)
    {
        if (rep.success)
        {
            runResultTypewriter.ShowText(runSuccessfulText);
        }

        else
        {
            runResultTypewriter.ShowText(runFailedText);
        }
        
        ShowRunStats(Singleton.Instance.playerStats.currentRunStats);
    }
}
