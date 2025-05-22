using UnityEngine;

public class MarkCabbageItemEffect : ItemEffect
{
    public double initialBonkValue = 1;
    public double scoreIncreaseOnMarkHit = 1;
    private double currentBonkValue = 1;

    public override void InitializeItemEffect()
    {
        GameStateMachine.PreBoardPopulateAction += PreBoardPopulateListener;
    }
    
    public override void DestroyItemEffect()
    {
        GameStateMachine.PreBoardPopulateAction += PreBoardPopulateListener;
    }
    
    private void PreBoardPopulateListener()
    {
        currentBonkValue = initialBonkValue;
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }

        Cabbage c = GameSingleton.Instance.gameStateMachine.GetRandomActiveCabbage();

        BonkMarker.MarkCabbage(c, currentBonkValue, owningItem);
        currentBonkValue += scoreIncreaseOnMarkHit;
    }

    public override string GetDescription()
    {
        return ($"Mark a random cabbage.  Bonking it with a ball bonks it for an additional {initialBonkValue:F0} and marks another cabbage." +
                $"This bonk value increases by {scoreIncreaseOnMarkHit:F0} with every mark hit.  Reset on round start.");
    }

    public override void RandomizePower()
    {
        initialBonkValue = Random.Range(1, 50);
        scoreIncreaseOnMarkHit = Random.Range(1, 50);
    }
}
