using System;
using UnityEngine;
using TMPro;

public class RunEndPanel : MenuPanel
{
    public TMP_Text totalBonksText;

    private void OnEnable()
    {
        RunManager.RunEndEvent += RunEndListener;
    }

    private void OnDisable()
    {
        RunManager.RunEndEvent -= RunEndListener;
    }

    public void ShowRunStats(RunStats stats)
    {
        totalBonksText.text = $"Cabbages Bonked: {Helpers.FormatWithSuffix(stats.totalBonks)}";
    }

    void RunEndListener(RunManager.RunEndParams rep)
    {
        ShowRunStats(Singleton.Instance.playerStats.currentRunStats);
    }
}
