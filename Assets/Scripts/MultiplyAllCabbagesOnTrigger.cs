using System;
using UnityEngine;
using System.Collections.Generic;

public class MultiplyAllCabbagesOnTrigger : MonoBehaviour
{
    public float scoreMultiplier = 2f;
    public PooledObjectData ballCollideVFX;
    public SFXInfo ballCollideSFX;
    public PooledObjectData scoreUpVFX;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball b = other.GetComponent<Ball>();
        if (b == null)
        {
            return;
        }

        List<Cabbage> cabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        foreach (Cabbage c in cabbages)
        {
            c.points *= scoreMultiplier;
            scoreUpVFX.Spawn(c.transform.position);
        }

        ballCollideSFX.Play();
        ballCollideVFX.Spawn(b.transform.position);
        b.KillBall();
    }
}
