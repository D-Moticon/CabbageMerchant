using UnityEngine;
using System.Collections.Generic;

public class IncreaseBallSizeEffect : ItemEffect
{
    public bool allBalls = false;
    public float scaleIncrease = 0.1f;
    public float scaleMax = 10f;
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (allBalls)
        {
            List<Ball> activeBalls = new List<Ball>(GameSingleton.Instance.gameStateMachine.activeBalls);
            foreach (Ball ball in activeBalls)
            {
               IncreaseBallSize(ball); 
            }
        }

        else
        {
            Ball b = tc.ball;
            if (b == null)
            {
                return;
            }

            IncreaseBallSize(b);
        }
        
    }

    void IncreaseBallSize(Ball b)
    {
        float oldSca = b.transform.localScale.x;
        float newSca = oldSca + scaleIncrease;
        b.transform.localScale = new Vector3(newSca, newSca, 1f);
    }
    
    public override string GetDescription()
    {
        return $"Increase size of ball by {scaleIncrease} up to {scaleMax}";
    }
}
