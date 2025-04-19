using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnType
    {
        Rate,
        Burst
    }

    public enum SpawnLocation
    {
        RandomPos,
        RandomCabbage
    }

    [Header("Spawn Settings")]
    [Tooltip("Pooled enemy to spawn")] public PooledObjectData enemyToSpawn;
    [Tooltip("Offset applied to the spawn bounds (local space)")] public Vector2 spawnOffset;
    [Tooltip("Local bounds within which to spawn enemies")] public Bounds spawnBounds;

    [Header("Spawn Modes")]
    public SpawnType spawnType = SpawnType.Rate;
    public SpawnLocation spawnLocation = SpawnLocation.RandomPos;
    [Tooltip("Enemies per second (used in Rate mode)")] public float firstRoundEnemiesPerSecond = 1f;
    [Tooltip("Total number to spawn at once (used in Burst mode)")] public int burstCount = 10;
    [Tooltip("Delay between individual spawns in a burst (seconds)")] public float burstInterval = 0.2f;

    [Header("Cabbage Spawn Offset")]
    [Tooltip("Vertical offset when spawning on top of a cabbage")]
    public float cabbageSpawnHeightOffset = 0.5f;

    [Header("Scaling (Rate mode)")]
    [Tooltip("Exponential scaling base")] public float epsBase = 0.5f;
    [Tooltip("Exponential scaling power")] public float epsPower = 1.5f;
    [Tooltip("Layer index for testing if MapSingleton missing")] public int testMapLayer = 1;

    private float enemiesPerSecond;
    private float secondsPerEnemy;
    private float spawnTimer;
    private bool spawning = false;
    private bool burstStarted = false;

    private void OnEnable()
    {
        GameStateMachine.BallFiredEvent += BallFiredListener;
        GameStateMachine.BoardFinishedPopulatingAction += BoardFinishedPopulatingListener;
    }

    private void OnDisable()
    {
        GameStateMachine.BallFiredEvent -= BallFiredListener;
        GameStateMachine.BoardFinishedPopulatingAction -= BoardFinishedPopulatingListener;
    }

    void Start()
    {
        int layer = testMapLayer;
        if (MapSingleton.Instance != null)
            layer = MapSingleton.Instance.mapManager.currentLayerIndex;

        enemiesPerSecond = firstRoundEnemiesPerSecond + epsBase * Mathf.Pow(layer, epsPower);
        secondsPerEnemy = 1f / enemiesPerSecond;
        spawnTimer = 0f;
        burstStarted = false;
    }

    void Update()
    {
        if (!spawning)
            return;

        if (spawnType == SpawnType.Rate)
        {
            spawnTimer += Time.deltaTime;
            while (spawnTimer >= secondsPerEnemy)
            {
                spawnTimer -= secondsPerEnemy;
                SpawnEnemy();
            }
        }
        else if (spawnType == SpawnType.Burst && !burstStarted)
        {
            burstStarted = true;
            StartCoroutine(BurstSpawnCoroutine());
        }
    }

    private IEnumerator BurstSpawnCoroutine()
    {
        for (int i = 0; i < burstCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void SpawnEnemy()
    {
        Vector3 worldPos;

        if (spawnLocation == SpawnLocation.RandomCabbage)
        {
            List<Cabbage> valid = new List<Cabbage>();
            foreach (var c in FindObjectsOfType<Cabbage>())
            {
                if (c.gameObject.activeInHierarchy && c.enabled)
                    valid.Add(c);
            }

            if (valid.Count == 0)
            {
                worldPos = GetRandomPosition();
            }
            else
            {
                var chosen = valid[Random.Range(0, valid.Count)];
                worldPos = chosen.transform.position + Vector3.up * cabbageSpawnHeightOffset;
            }
        }
        else // RandomPos
        {
            worldPos = GetRandomPosition();
        }

        var enemy = enemyToSpawn.Spawn();
        enemy.transform.position = worldPos;
    }

    private Vector3 GetRandomPosition()
    {
        Vector2 ofs = new Vector2(
            Random.Range(-spawnBounds.extents.x, spawnBounds.extents.x),
            Random.Range(-spawnBounds.extents.y, spawnBounds.extents.y)
        );
        Vector3 localPos = spawnBounds.center + (Vector3)spawnOffset + (Vector3)ofs;
        return transform.TransformPoint(localPos);
    }

    private void BallFiredListener(Ball ball)
    {
        if (spawnType == SpawnType.Rate)
            spawning = true;
    }

    private void BoardFinishedPopulatingListener()
    {
        if (spawnType == SpawnType.Burst)
            spawning = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.TransformPoint(spawnBounds.center + (Vector3)spawnOffset);
        Gizmos.DrawWireCube(center, spawnBounds.size);
    }
}
