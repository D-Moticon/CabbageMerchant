using System;
using UnityEngine;
using TMPro;

public class RerollButton : ClickableObject
{
    public ShopManager[] shopManagers;
    public TMP_Text rollsRemainingText;
    public TMP_Text rollCostText;
    
    public override void TryClick()
    {
        if (shopManagers[0].rerollsRemaining <= 0)
        {
            FailClick("No rerolls remaining!");
            return;
        }

        if (Singleton.Instance.playerStats.coins < Singleton.Instance.playerStats.reRollCost)
        {
            FailClick("Not enough money!");
        }
        
        Click();
    }

    public override void Click()
    {
        base.Click();
        foreach (var shopManager in shopManagers)
        {
            shopManager.ReRoll();
        }
    }

    private void Update()
    {
        rollsRemainingText.text = $"{shopManagers[0].rerollsRemaining.ToString()} remaining";
        rollCostText.text = $"{Singleton.Instance.playerStats.reRollCost}";
    }
}
