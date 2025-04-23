using UnityEngine;
using System.Collections;

public class BallBounceUpwardItemEffect : ItemEffect
{
    public float speedMult = 1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null || tc.ball == null || GameSingleton.Instance == null)
        {
            //Checking game singleton to prevent golf ball bounces
            return;
        }

        Vector2 vel = tc.ball.rb.linearVelocity;
        if (vel.y < 0)
        {
            vel = new Vector2(vel.x, -vel.y);
            tc.ball.rb.linearVelocity = vel*speedMult;
        }
    }

    public override string GetDescription()
    {
        string s = $"Bounce the ball upward if it would bounce downward";
        if (speedMult > 1.001f)
        {
            //s += "\n";
            //s += $"Speed boost: {speedMult:F1}x";
        }

        return (s);
    }
    
}
