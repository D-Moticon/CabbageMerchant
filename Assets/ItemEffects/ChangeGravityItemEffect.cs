using UnityEngine;

public class ChangeGravityItemEffect : ItemEffect
{
    public Vector2 newGravity = new Vector2(0f,-9.81f);
    
    
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Physics2D.gravity = newGravity;
    }

    public override string GetDescription()
    {
        string grav = "";
        if (Mathf.Abs(newGravity.x) < 0.1f && Mathf.Abs(newGravity.y)>0.1f)
        {
            grav = Helpers.ToPercentageString(1f-Mathf.Abs(newGravity.y / 9.81f));
            return ($"Reduce gravity by {grav}");
        }
        else if (Mathf.Abs(newGravity.x) < 0.1f && Mathf.Abs(newGravity.y) < 0.1f)
        {
            return ($"Turn off gravity");
        }
        else
        {
            return "";
        }
    }
}
