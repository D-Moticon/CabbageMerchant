using UnityEngine;
using System.Collections;

public class TimeElapsedTrigger : Trigger
{
    public float duration = 5f;
    private static Vector2 randomizeDurationRange = new Vector2(0.3f, 4f);
    
    // our running loop handle
    private Coroutine loopCoroutine;

    public override void InitializeTrigger(Item item)
    {
        GameStateMachine.BallFiredEvent           += BallFiredListener;
        GameStateMachine.ExitingBounceStateAction += BounceStateExitedListener;
    }

    public override void RemoveTrigger(Item item)
    {
        GameStateMachine.BallFiredEvent           -= BallFiredListener;
        GameStateMachine.ExitingBounceStateAction -= BounceStateExitedListener;
        StopLoop();
    }

    void BallFiredListener(Ball b)
    {
        // whenever the ball is (re-)fired, start a fresh loop
        StopLoop();
        loopCoroutine = Singleton.Instance
            .StartCoroutine(TriggerWhileInPlay());
    }

    void BounceStateExitedListener()
    {
        // as soon as bounce state ends, kill the loop
        StopLoop();
    }

    IEnumerator TriggerWhileInPlay()
    {
        // keep firing every duration until we’re told to stop
        while (true)
        {
            yield return new WaitForSeconds(duration);
            DoMyTriggerAction();
        }
    }

    void StopLoop()
    {
        if (loopCoroutine != null)
        {
            Singleton.Instance.StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
    }

    void DoMyTriggerAction()
    {
        TriggerContext tc = new TriggerContext();
        if (GameSingleton.Instance != null && GameSingleton.Instance.gameStateMachine.activeBalls.Count > 0)
        {
            int rand = Random.Range(0, GameSingleton.Instance.gameStateMachine.activeBalls.Count);
            tc.ball = GameSingleton.Instance.gameStateMachine.activeBalls[rand];
            tc.point = tc.ball.transform.position;
            float randAng = Random.Range(0f, 360f);
            Vector2 randDir = Helpers.AngleDegToVector2(randAng);
            tc.normal = randDir;
        }

        owningItem.TryTriggerItem(tc);
    }

    public override string GetTriggerDescription()
        => $"Every {duration:F1}s while ball is in play";
    
    public override void RandomizeTrigger()
    {
        base.RandomizeTrigger();
        duration = Random.Range(randomizeDurationRange.x, randomizeDurationRange.y);
    }
}
