using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnType { Rate, Burst }
    public enum SpawnLocation { RandomPos, RandomCabbage }

    [Header("Spawn Settings")]
    [Tooltip("Pooled enemy to spawn")] public PooledObjectData enemyToSpawn;
    public PooledObjectData spawnVFX;
    public SFXInfo spawnSFX;
    [Tooltip("Offset applied to the spawn bounds (local space)")] public Vector2 spawnOffset;
    [Tooltip("Local bounds within which to spawn enemies")] public Bounds spawnBounds;

    [Header("Spawn Modes")]
    public SpawnType spawnType = SpawnType.Rate;
    public SpawnLocation spawnLocation = SpawnLocation.RandomPos;
    [Tooltip("Enemies per second (used in Rate mode)")] public float firstRoundEnemiesPerSecond = 1f;

    [Header("Burst Settings")]
    [Tooltip("Total number to spawn at once (used in Burst mode)")] public int burstCount = 10;
    [Tooltip("Delay between individual spawns in a burst (seconds)")] public float burstInterval = 0.2f;
    [Tooltip("Minimum distance between burst spawn positions")] public float burstMinSeparation = 1f;

    [Header("Cabbage Spawn Offset")]
    [Tooltip("Vertical offset when spawning on top of a cabbage")] public float cabbageSpawnHeightOffset = 0.5f;

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
        int layer = Singleton.Instance.playerStats.currentMapLayer;
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
                SpawnEnemyAt(GetRandomSpawnPosition());
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
        var positions = new List<Vector3>();
        for (int i = 0; i < burstCount; i++)
        {
            Vector3 pos = GetRandomSpawnPosition();
            if (burstMinSeparation > 0f && positions.Count > 0)
            {
                // try to enforce minimum separation
                const int maxTries = 10;
                int tries = 0;
                while (tries < maxTries)
                {
                    bool tooClose = false;
                    foreach (var prev in positions)
                    {
                        if ((pos - prev).sqrMagnitude < burstMinSeparation * burstMinSeparation)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (!tooClose)
                        break;
                    pos = GetRandomSpawnPosition();
                    tries++;
                }
            }
            positions.Add(pos);
            SpawnEnemyAt(pos);
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void SpawnEnemyAt(Vector3 worldPos)
    {
        var enemy = enemyToSpawn.Spawn();
        enemy.transform.position = worldPos;
        spawnSFX.Play();
        if (spawnVFX != null)
            spawnVFX.Spawn(worldPos);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnLocation == SpawnLocation.RandomCabbage)
        {
            var valid = new List<Cabbage>();
            foreach (var c in FindObjectsOfType<Cabbage>())
                if (c.gameObject.activeInHierarchy && c.enabled)
                    valid.Add(c);
            if (valid.Count > 0)
            {
                var chosen = valid[Random.Range(0, valid.Count)];
                return chosen.transform.position + Vector3.up * cabbageSpawnHeightOffset;
            }
        }
        // RandomPos fallback
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
