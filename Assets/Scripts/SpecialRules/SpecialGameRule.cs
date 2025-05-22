using UnityEngine;

[System.Serializable]
public abstract class SpecialGameRule
{
    protected GameStateMachine gameStateMachine;
    
    public void InitializeRule(GameStateMachine gsm)
    {
        gameStateMachine = gsm;
        GameStateMachine.PreBoardPopulateAction += PreBoardPopulate;
        GameStateMachine.BoardFinishedPopulatingAction += PostBoardPopulate;
    }

    public void RemoveRule()
    {
        GameStateMachine.PreBoardPopulateAction -= PreBoardPopulate;
        GameStateMachine.BoardFinishedPopulatingAction -= PostBoardPopulate;
    }
    
    public virtual void PreBoardPopulate()
    {
    }

    public virtual void PostBoardPopulate()
    {
    }
}
