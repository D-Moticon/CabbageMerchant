using UnityEngine;

public class ForceNextBallFireEffect : ItemEffect
{
    public Vector2Int stackRange = new Vector2Int(10, 1000);
    private int stacksToAdd;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Singleton.Instance.launchModifierManager.ForceNextBallFire(stacksToAdd);
    }

    public override string GetDescription()
    {
        return ($"<fire>Scoville Level {stacksToAdd}</fire>: Add {stacksToAdd} stacks of <fire>fire</fire> to next ball (<fire>Fire</fire> bonks cabbages over time and can spread to balls on contact");
    }

    public override void InitializeItemEffect()
    {
        base.InitializeItemEffect();
        stacksToAdd = Random.Range(stackRange.x, stackRange.y);
    }
}
