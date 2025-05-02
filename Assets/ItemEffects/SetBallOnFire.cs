using UnityEngine;

public class SetBallOnFire : ItemEffect
{
    public int numberStacks = 6;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null || tc.ball == null)
        {
            return;
        }
        
        BallFire.SetBallOnFire(tc.ball, numberStacks);
    }

    public override string GetDescription()
    {
        return ($"Set ball on <fire>fire</fire> ({numberStacks} stacks)");
    }
}
