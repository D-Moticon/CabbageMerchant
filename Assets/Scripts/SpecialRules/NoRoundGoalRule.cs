using UnityEngine;

[System.Serializable]
public class NoRoundGoalRule : SpecialGameRule
{
    public override void PreBoardPopulate()
    {
        base.PreBoardPopulate();
        gameStateMachine.SetNoRoundGoal();
    }
}
