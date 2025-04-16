using System;
using UnityEngine;
using TMPro;

public class RerollButton : ClickableObject
{
    public ShopManager shopManager;
    public TMP_Text rollsRemainingText;
    public TMP_Text rollCostText;
    
    public override void TryClick()
    {
        if (shopManager.rerollsRemaining <= 0)
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
        shopManager.ReRoll();
    }

    private void Update()
    {
        rollsRemainingText.text = $"{shopManager.rerollsRemaining.ToString()} remaining";
        rollCostText.text = $"{Singleton.Instance.playerStats.reRollCost}";
    }
}
