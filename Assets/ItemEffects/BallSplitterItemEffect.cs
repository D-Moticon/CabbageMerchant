using UnityEngine;

public class BallSplitterItemEffect : ItemEffect
{
    public PooledObjectData ballPooledObject;
    public string ballName;
    public string ballDescription;
    public int numberCopies = 1;
    public float ballLaunchSpeed;
    public Vector2 ballLaunchAngleRange;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (tc == null)
        {
            tc = new TriggerContext();
        }
        
        if (tc.ball == null)
        {
            int rand = Random.Range(0, GameSingleton.Instance.gameStateMachine.activeBalls.Count);
            tc.ball = GameSingleton.Instance.gameStateMachine.activeBalls[rand];
            tc.point = tc.ball.transform.position;
            float randAng = Random.Range(0f, 360f);
            Vector2 randDir = Helpers.AngleDegToVector2(randAng);
            tc.normal = randDir;
        }

        else
        {
            for (int i = 0; i < numberCopies; i++)
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

    public override string GetDescription()
    {
        string desc = "";
        if (!string.IsNullOrEmpty(ballDescription))
        {
            desc = $"that {ballDescription}";
        }

        string plural = "";
        if (numberCopies > 1)
        {
            plural = "s";
        }
        
        return ($"Split ball into {numberCopies} extra {ballName}{plural} {desc}");
    }
}
