using UnityEngine;

public class BallSpawnedTrigger : Trigger
{
    public override void InitializeTrigger(Item item)
    {
        Ball.BallEnabledEvent += OnBallSpawned;
    }

    public override void RemoveTrigger(Item item)
    {
        Ball.BallEnabledEvent -= OnBallSpawned;
    }

    public override string GetTriggerDescription()
    {
        return ("Any ball spawned");
    }

    void OnBallSpawned(Ball ball)
    {
        TriggerContext tc = new TriggerContext();
        tc.ball = ball;
        owningItem.TryTriggerItem(tc);
    }
}
