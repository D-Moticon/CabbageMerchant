using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnOverTimeDuringBounce : MonoBehaviour
{
    public PooledObjectData objToSpawn;

    public Vector2 secondsPerSpawnRange = new Vector2(0.5f,1f);
    private float secondsPerSpawn = 0.5f;
    public PooledObjectData spawnVFX;
    public SFXInfo spawnSFX;

    private float spawnCountdown;

    private bool isSpawning = false;

    private void OnEnable()
    {
        GameStateMachine.EnteringBounceStateAction += OnBounceStateEntered;
        GameStateMachine.ExitingBounceStateAction += OnBounceStateExited;

        secondsPerSpawn = Random.Range(secondsPerSpawnRange.x, secondsPerSpawnRange.y);
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringBounceStateAction -= OnBounceStateEntered;
        GameStateMachine.ExitingBounceStateAction -= OnBounceStateExited;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawning)
        {
            return;
        }

        spawnCountdown -= Time.deltaTime;
        if (spawnCountdown <= 0)
        {
            SpawnObject();
            spawnCountdown = secondsPerSpawn;
        }
    }

    void SpawnObject()
    {
        objToSpawn.Spawn(this.transform.position);

        if (spawnVFX != null)
        {
            spawnVFX.Spawn(this.transform.position);
        }
        
        spawnSFX.Play();
    }

    void OnBounceStateEntered()
    {
        isSpawning = true;
        spawnCountdown = secondsPerSpawn;
    }

    void OnBounceStateExited()
    {
        isSpawning = false;
    }
}
