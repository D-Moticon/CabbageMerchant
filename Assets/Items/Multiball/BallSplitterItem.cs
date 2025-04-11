using UnityEditor;
using UnityEngine;

public class BallSplitterItem : Item
{
    public PooledObjectData ballPooledObject;
    public float ballLaunchSpeed;
    public Vector2 ballLaunchAngleRange;
    protected override void TriggerItem(TriggerContext tc = null)
    {
        base.TriggerItem();
        
        if (tc == null)
        {
            
        }

        else
        {
            Ball b = ballPooledObject.Spawn(tc.ball.transform.position, Quaternion.identity).GetComponent<Ball>();
            float ang = Random.Range(ballLaunchAngleRange.x, ballLaunchAngleRange.y);
            float speed = ballLaunchSpeed;
            Vector2 dir = Helpers.AngleDegToVector2(ang);
            Vector2 vel = speed * dir;
            b.rb.linearVelocity = vel;
        }
        
    }
}
