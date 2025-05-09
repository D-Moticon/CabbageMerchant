using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallBounceTrigger : Trigger
{
    public int numberBounces = 1;
    private int bounceCount = 0;

    public enum BounceType
    {
        any,
        cabbageOnly,
        wallOnly
    }

    public BounceType bounceType;

    private static Vector2Int randomizeNumberBouncesRange = new Vector2Int(1, 10);
    
    public override void InitializeTrigger(Item item)
    {
        Ball.BallCollidedEvent += BallCollidedListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
    }

    public override void RemoveTrigger(Item item)
    {
        Ball.BallCollidedEvent -= BallCollidedListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
    }

    public override string GetTriggerDescription()
    {
        string plural = "";
        if (numberBounces > 1)
        {
            plural = "s";
        }

        string bounceTypeString = "";
        
        switch (bounceType)
        {
            case BounceType.any:
                break;
            case BounceType.cabbageOnly:
                bounceTypeString = "(cabbages only)";
                break;
            case BounceType.wallOnly:
                bounceTypeString = "(walls only)";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return ($"Every {numberBounces} ball bounce{plural} {bounceTypeString}");
    }

    void BallCollidedListener(Ball b, Collision2D col)
    {
        if (itemHasTriggeredThisFrame)
        {
            return;
        }
        
        switch (bounceType)
        {
            case BounceType.any:
                break;
            case BounceType.cabbageOnly:
                if (col.gameObject.GetComponent<Cabbage>() == null)
                {
                    return;
                }
                else
                {
                    break;
                }
            case BounceType.wallOnly:
                if (col.gameObject.GetComponent<Cabbage>() != null)
                {
                    return;
                }
                else
                {
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        bounceCount++;
        if (bounceCount >= numberBounces)
        {
            TriggerContext tc = new TriggerContext();
            tc.ball = b;
            tc.point = col.contacts[0].point;
            tc.normal = col.contacts[0].normal;
            tc.relativeVelocity = col.relativeVelocity;
            owningItem.TryTriggerItem(tc);
            bounceCount = 0;
        }
    }

    void BallFiredListener(Ball b)
    {
        bounceCount = 0;
    }

    public override void RandomizeTrigger()
    {
        numberBounces = Random.Range(randomizeNumberBouncesRange.x, randomizeNumberBouncesRange.y);
    }
}
