using System;
using UnityEngine;
using System.Collections.Generic;

public class SpecialRuleManager : MonoBehaviour
{
    [SerializeReference] public List<SpecialGameRule> specialGameRules = new List<SpecialGameRule>();


    private void OnEnable()
    {
        GameStateMachine.GSM_Enabled_Event += GSMEnabledListener;
    }

    private void OnDisable()
    {
        GameStateMachine.GSM_Enabled_Event -= GSMEnabledListener;
        
        ClearSpecialRules();
    }

    void GSMEnabledListener(GameStateMachine gsm)
    {
        InitializeRules(gsm);
    }
    
    public void ClearSpecialRules()
    {
        foreach (var rule in specialGameRules)
        {
            rule.RemoveRule();
        }
        
        specialGameRules.Clear();
    }

    public void AddSpecialGameRule(SpecialGameRule sgr)
    {
        if (specialGameRules == null)
        {
            specialGameRules = new List<SpecialGameRule>();
        }

        SpecialGameRule clone = Helpers.DeepClone(sgr);
        specialGameRules.Add(clone);
    }

    public void InitializeRules(GameStateMachine gsm)
    {
        foreach (var rule in specialGameRules)
        {
            rule.InitializeRule(gsm);
        }
    }
}
