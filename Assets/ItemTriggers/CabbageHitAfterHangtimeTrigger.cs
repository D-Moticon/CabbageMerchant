using UnityEngine;

public class CabbageHitAfterHangtimeTrigger : Trigger
{
    public float minHangtime = 1f;
    private static Vector2 randomizeMinHangtimeRange = new Vector2(0.30f, 1.25f);
    
    public override void InitializeTrigger(Item item)
    {
        Ball.BallHitCabbageEvent += CabbageHitListener;
    }

    public override void RemoveTrigger(Item item)
    {
        Ball.BallHitCabbageEvent -= CabbageHitListener;
    }

    public override string GetTriggerDescription()
    {
        string s = $"Ball bonks cabbage after {minHangtime:F1} seconds of air time";
        return s;
    }

    void CabbageHitListener(Ball.BallHitCabbageParams bcParams)
    {
        if (bcParams.ball != null && bcParams.hangTimeBeforeHit >= minHangtime)
        {
            TriggerContext tc = new TriggerContext();
            tc.ball = bcParams.ball;
            tc.cabbage = bcParams.cabbage;
            tc.point = bcParams.point;
            tc.normal = bcParams.normal;
            
            owningItem.TryTriggerItem(tc);
        }
    }

    public override void RandomizeTrigger()
    {
        base.RandomizeTrigger();
        minHangtime = Random.Range(randomizeMinHangtimeRange.x, randomizeMinHangtimeRange.y);
    }
}
