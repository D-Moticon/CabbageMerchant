using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAndSpinObjectsInArray : ItemEffect
{
    [Header("Spawn Settings")]
    public bool allBalls = false;
    [Tooltip("What to spawn")]
    public PooledObjectData objectToSpawn;
    [Tooltip("How many to spawn around the ball")]
    public int quantityToSpawn = 2;

    [Header("Spin Settings")]
    [Tooltip("Angular speed in degrees per second")]
    public float angularSpeed = 300f;
    [Tooltip("How long the effect lasts")]
    public float duration = 1f;
    
    public string objectName;
    public string objectDescription;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        if (allBalls)
        {
            List<Ball> activeBalls = new List<Ball>(GameSingleton.Instance.gameStateMachine.activeBalls);
            foreach (Ball ball in activeBalls)
            {
                GameSingleton.Instance.gameStateMachine.StartCoroutine(SpawnAndSpinCoroutine(ball)); 
            }
        }

        else
        {
            Ball b = tc.ball;
            if (b == null) return;
            GameSingleton.Instance.gameStateMachine.StartCoroutine(SpawnAndSpinCoroutine(b));
        }
        
    }

    private IEnumerator SpawnAndSpinCoroutine(Ball ball)
    {
        Vector3 center = ball.transform.position;

        // spawn objects all at ball center with different initial Z rotations
        var spawned = new List<GameObject>(quantityToSpawn);
        for (int i = 0; i < quantityToSpawn; i++)
        {
            float initialAngle = 360f * i / quantityToSpawn;
            Quaternion rot = Quaternion.Euler(0f, 0f, initialAngle);
            GameObject go = objectToSpawn.Spawn(center, rot);
            spawned.Add(go);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            center = ball.transform.position;

            // keep each at the ball and spin in place
            foreach (var go in spawned)
            {
                if (go == null) continue;
                go.transform.position = center;
                go.transform.Rotate(0f, 0f, angularSpeed * Time.deltaTime, Space.Self);
            }

            yield return null;
        }

        // deactivate them
        foreach (var go in spawned)
            if (go != null)
                go.SetActive(false);
    }

    public override string GetDescription()
    {
        string ballString = "the ball's position";
        if (allBalls)
        {
            ballString = "every ball's position";
        }
        
        return
            $"Spawn {quantityToSpawn} {objectName} at {ballString} that spin and {objectDescription} for {duration}s";
    }
}
