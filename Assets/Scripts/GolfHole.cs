using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;

public class GolfHole : MonoBehaviour
{
    [SerializeReference]
    public Prize prize;

    public SpriteRenderer sr;
    public PooledObjectData scoreVFX;
    public SFXInfo scoreSFX;

    public delegate void PrizeDelegate(Prize p);
    public static event PrizeDelegate PrizeScored;
    
    public Vector2 startXRand = new Vector2(-0.5f, 0.5f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float rand = Random.Range(startXRand.x, startXRand.y);
        transform.position += rand * Vector3.right;
    }

    public void SetPrize(Prize p)
    {
        prize = p;
        sr.sprite = prize.icon;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball b = other.GetComponent<Ball>();
        if (b != null)
        {
            if (scoreVFX != null)
            {
                scoreVFX.Spawn(this.transform.position);
                scoreSFX.Play();
            }
            
            b.KillBall();
            PrizeScored?.Invoke(prize);
        }

    }
}
