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

        if (GameSingleton.Instance.gameStateMachine.activeBalls.Count == 0)
        {
            return;
        }

        Vector2 pos = Vector2.zero;
        
        if (tc.ball == null)
        {
            int rand = Random.Range(0, GameSingleton.Instance.gameStateMachine.activeBalls.Count);
            if (GameSingleton.Instance.gameStateMachine.activeBalls.Count > 0)
            {
                tc.ball = GameSingleton.Instance.gameStateMachine.activeBalls[rand];
                tc.point = tc.ball.transform.position;
                pos = tc.ball.transform.position;
            }

            else
            {
                pos = new Vector2(Random.Range(-4f,4f), Random.Range(-4f,4f));
            }
            
            float randAng = Random.Range(0f, 360f);
            Vector2 randDir = Helpers.AngleDegToVector2(randAng);
            tc.normal = randDir;
        }

        else
        {
            pos = tc.ball.transform.position;
        }

        
        for (int i = 0; i < numberCopies; i++)
        {
            Ball b = ballPooledObject.Spawn(pos, Quaternion.identity).GetComponent<Ball>();
            float ang = Random.Range(ballLaunchAngleRange.x, ballLaunchAngleRange.y);
            float speed = ballLaunchSpeed;
            Vector2 dir = Helpers.AngleDegToVector2(ang);
            Vector2 vel = speed * dir;
            b.rb.linearVelocity = vel;

            if (tc.ball != null)
            {
                b.rb.sharedMaterial = tc.ball.rb.sharedMaterial;
                b.col.sharedMaterial = tc.ball.col.sharedMaterial;
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
