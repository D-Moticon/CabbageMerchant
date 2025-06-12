using UnityEngine;

public class BallSpawnedTrigger : Trigger
{
    private bool isHandling = false; //infinite loop preventer
    
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
        if (isHandling) return;
        isHandling = true;
        
        TriggerContext tc = new TriggerContext();
        tc.ball = ball;
        owningItem.TryTriggerItem(tc);

        isHandling = false;
    }
}
