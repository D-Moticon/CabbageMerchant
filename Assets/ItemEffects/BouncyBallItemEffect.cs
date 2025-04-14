using UnityEngine;

public class BouncyBallItemEffect : ItemEffect
{
    public float bouncinessMult = 1f;
    public string bouncinessAdverb = "";
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null)
        {
            return;
        }

        if (tc.ball == null || tc.normal == null)
        {
            return;
        }

        if (tc.cabbage != null)
        {
            //walls only
            return;
        }
        
        // Calculate a new velocity by reflecting the relative velocity over the collision normal,
        // then multiply by bouncinessMult for extra bounce.
        Vector2 newVelocity = Vector2.Reflect(-tc.relativeVelocity, tc.normal) * bouncinessMult;
    
        // Set the ball’s linear velocity to the calculated value.
        tc.ball.rb.linearVelocity = newVelocity;
    }

    public override string GetDescription()
    {
        return ($"Make the ball bounce {bouncinessAdverb} hard");
    }
}
