using UnityEngine;

public class BallSplitterItemEffect : ItemEffect
{
    public PooledObjectData ballPooledObject;
    public string ballName;
    public string ballDescription;
    public int numberCopies = 1;
    public float ballLaunchSpeed;
    public Vector2 ballLaunchAngleRange;
    public float multiplyScale = 1f;
    public float multiplyBonkValue = 1f;
    public bool destroyOriginalBall = false;
    
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
        PooledObjectData objToSpawn = ballPooledObject;
        float sca = 1f;
        float bonkValue = 1f;
        
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
            if (tc.ball.pooledObjectRef != null)
            {
                objToSpawn = tc.ball.pooledObjectRef;
            }

            sca = tc.ball.transform.localScale.x;
            bonkValue = tc.ball.bonkValue;
        }

        sca *= multiplyScale;
        
        for (int i = 0; i < numberCopies; i++)
        {
            Ball b = objToSpawn.Spawn(pos, Quaternion.identity).GetComponent<Ball>();
            float ang = Random.Range(ballLaunchAngleRange.x, ballLaunchAngleRange.y);
            float speed = ballLaunchSpeed;
            Vector2 dir = Helpers.AngleDegToVector2(ang);
            Vector2 vel = speed * dir;
            b.rb.linearVelocity = vel;
            
            if (sca < 0.05f)
            {
                sca = 0.05f;
            }

            if (sca > 20f)
            {
                sca = 20f;
            }
            b.SetScale(sca);
            b.bonkValue = bonkValue*multiplyBonkValue;

            if (tc.ball != null)
            {
                b.rb.sharedMaterial = tc.ball.rb.sharedMaterial;
                b.col.sharedMaterial = tc.ball.col.sharedMaterial;
            }

            BallFire bf = tc.ball.GetComponentInChildren<BallFire>();
            if (bf != null)
            {
                BallFire.SetBallOnFire(b, bf.GetStacksRemaining());
            }
        }

        if (destroyOriginalBall && tc.ball != null)
        {
            tc.ball.DestroyBall();
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

    public override void RandomizePower()
    {
        base.RandomizePower();
        numberCopies = Random.Range(1, 8);
    }
}
