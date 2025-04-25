using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

/// <summary>
/// Leaf blower weapon: rapidly fires projectiles from every active ball towards the mouse,
/// with a visual blower sprite attached to each ball during firing.
/// Supports configurable number of projectiles per shot and spread angle.
/// </summary>
public class Weapon_LeafBlower_ItemEffect : ItemEffect
{
    [Header("Projectile Settings")]
    [Tooltip("Projectile prefab to spawn")]                   public PooledObjectData projectilePrefab;
    [Tooltip("Speed at which projectiles travel")]            public float projectileSpeed = 20f;
    [Tooltip("Number of projectiles per ball each shot")]     public int projectilesPerShot = 1;
    [Tooltip("Maximum deviation angle per projectile (degrees)")] public float spreadAngle = 10f;

    [Header("Fire Settings")]
    [Tooltip("Shots fired per second")]                      public float fireRate = 15f;
    [Tooltip("Total duration of the leaf blower effect")]    public float effectDuration = 2f;

    [Header("Visual Blower Sprite")]
    [Tooltip("Sprite prefab to parent to each ball")]         public PooledObjectData blowerSpritePrefab;

    [Header("Audio")]
    [Tooltip("Looping SFX for leaf blower while active")]     public SFXInfo leafBlowerSFX;
    [Tooltip("Sound to play for each individual shot")]       public SFXInfo sfxPerShot;
    private EventInstance leafBlowerSFXInstance;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        var balls = GameSingleton.Instance.gameStateMachine.activeBalls;

        // spawn and parent blower sprite under each ball
        var spriteMap = new Dictionary<Ball, GameObject>();
        foreach (var ball in balls)
        {
            var go = blowerSpritePrefab.Spawn();
            go.transform.SetParent(ball.transform, false);
            go.transform.localPosition = Vector3.zero;
            spriteMap[ball] = go;
        }

        // start firing coroutine
        GameSingleton.Instance.gameStateMachine.StartCoroutine(
            LeafBlowerCoroutine(balls, spriteMap)
        );

        // play looping SFX
        if (leafBlowerSFXInstance.isValid())
            leafBlowerSFXInstance.stop(STOP_MODE.IMMEDIATE);
        leafBlowerSFXInstance = leafBlowerSFX.Play(effectDuration);
    }

    private IEnumerator LeafBlowerCoroutine(
        List<Ball> balls,
        Dictionary<Ball, GameObject> spriteMap
    )
    {
        float elapsed       = 0f;
        float interval      = 1f / fireRate;
        float timeSinceShot = 0f;

        while (elapsed < effectDuration)
        {
            if (Singleton.Instance.playerInputManager.weaponFireUp)
            {
                if (elapsed < 0.2f)
                {
                    Singleton.Instance.gameHintManager.QueueHintUntilBouncingDone("Looks like I need to hold the weapon button to get the full effect!");
                }
                
                break;
            }

            // remove destroyed balls & sprites
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                if (!balls[i].gameObject.activeInHierarchy)
                {
                    if (spriteMap.TryGetValue(balls[i], out var oldGo))
                        oldGo.SetActive(false);
                    balls.RemoveAt(i);
                }
            }
            if (balls.Count == 0)
                break;

            float dt = Time.deltaTime;
            elapsed       += dt;
            timeSinceShot += dt;

            // aim blower sprite at mouse
            Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
            foreach (var ball in balls)
            {
                if (spriteMap.TryGetValue(ball, out var go) && go.activeInHierarchy)
                {
                    Vector2 dir = (mousePos - (Vector2)ball.transform.position).normalized;
                    go.transform.right = dir;
                }
            }

            if (timeSinceShot >= interval)
            {
                // fire projectiles per ball
                foreach (var ball in balls)
                {
                    Vector2 baseDir = (mousePos - (Vector2)ball.transform.position).normalized;
                    for (int j = 0; j < projectilesPerShot; j++)
                    {
                        var proj = projectilePrefab.Spawn();
                        proj.transform.position = ball.transform.position;
                        if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                        {
                            float halfSpread = spreadAngle * 0.5f;
                            float angleOffset = Random.Range(-halfSpread, halfSpread);
                            float rad = angleOffset * Mathf.Deg2Rad;
                            Vector2 dir = new Vector2(
                                baseDir.x * Mathf.Cos(rad) - baseDir.y * Mathf.Sin(rad),
                                baseDir.x * Mathf.Sin(rad) + baseDir.y * Mathf.Cos(rad)
                            );
                            rb.linearVelocity = dir * projectileSpeed;
                        }
                        sfxPerShot.Play();
                    }
                }
                timeSinceShot -= interval;
            }

            yield return null;
        }

        // cleanup blower sprites
        foreach (var go in spriteMap.Values)
        {
            if (go != null)
            {
                go.transform.SetParent(null);
                go.SetActive(false);
            }
        }

        // stop looping SFX
        if (leafBlowerSFXInstance.isValid())
            leafBlowerSFXInstance.stop(STOP_MODE.IMMEDIATE);
    }

    public override string GetDescription()
    {
        return $"Hold to fire " +
            $"{projectilesPerShot} leaves/ball @ {fireRate:F1} shots/sec " +
            $"for {effectDuration:F1}s";
    }
}
