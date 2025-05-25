using UnityEngine;

public class BallPhysicsMaterialItemEffect : ItemEffect
{
    public PhysicsMaterial2D newPhyiscsMaterial;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null)
        {
            return;
        }
        
        Ball b = tc.ball;
        if (b == null)
        {
            return;
        }

        b.rb.sharedMaterial = newPhyiscsMaterial;
        b.col.sharedMaterial = newPhyiscsMaterial;
    }

    public override string GetDescription()
    {
        return ("Increase ball bounciness");
    }
}
