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

        /*List<Cabbage> cabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        foreach (Cabbage c in cabbages)
        {
            c.points *= scoreMultiplier;
            scoreUpVFX.Spawn(c.transform.position);
        }*/
        
        GameSingleton.Instance.gameStateMachine.MultiplyRoundScore(scoreMultiplier);
        Singleton.Instance.floaterManager.SpawnPopFloater($"Round Score x{scoreMultiplier:F1}!",this.transform.position,Color.green, 2f);

        ballCollideSFX.Play();
        ballCollideVFX.Spawn(b.transform.position);
        b.KillBall();
    }
}
