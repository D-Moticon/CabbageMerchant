using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMOD.Studio;

public class Weapon_Sprinkler_ItemEffect : ItemEffect
{
    [Header("What to spawn")]
    public PooledObjectData objectToSpawn;

    [Header("Visual Sprinkler")]
    [Tooltip("A little rotating sprite to show the effect on each ball")]
    public PooledObjectData sprinklerSprite;

    [Header("Pattern settings")]
    [Tooltip("Degrees per second to rotate the sprinkler pattern")]
    public float angularSpeed = 100f;

    [Tooltip("Linear speed to fire each projectile")]
    public float fireSpeed = 20f;
    [Tooltip("How many spawn‑bursts per second")]
    public float spawnsPerSecond = 5f;
    [Tooltip("How many objects per burst (evenly around circle)")]
    public int numberObjectsPerSpawn = 4;
    [Tooltip("Total duration of the effect, in seconds)")]
    public float sprinklerDuration = 1f;

    public SFXInfo sprinkerSFX;
    private static bool releaseOnFireUp = true;
    private EventInstance sprinklerSFXInstance;

    public string spawnedWeaponName;
    public string projectileName;
    public string projectileDescription;

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // grab all currently active balls
        var balls = GameSingleton.Instance.gameStateMachine.activeBalls;

        // spawn & parent a little sprinkler sprite under each ball
        var spriteMap = new Dictionary<Ball, GameObject>();
        foreach (var ball in balls)
        {
            var go = sprinklerSprite.Spawn();
            go.transform.SetParent(ball.transform, false);
            go.transform.localPosition = Vector3.zero;
            spriteMap[ball] = go;
        }

        // start the coroutine on the GameStateMachine MB
        GameSingleton.Instance.gameStateMachine
            .StartCoroutine(SprinklerCoroutine(balls, spriteMap));

        // play SFX for exactly the duration
        if (sprinklerSFXInstance.isValid())
        {
            sprinklerSFXInstance.stop(STOP_MODE.IMMEDIATE);
        }
        sprinklerSFXInstance = sprinkerSFX.Play(sprinklerDuration);
    }

    public override string GetDescription()
    {
        string desc =
            $"Balls turn into {spawnedWeaponName}s that spawn {projectileName}s that {projectileDescription}" +
            $"\n Nozzles: {numberObjectsPerSpawn}" +
            $"\n Spawns Per Second: {spawnsPerSecond:F0} * WP" +
            $"\n Duration: {sprinklerDuration}s";
        
        
        return desc;
    }

    private IEnumerator SprinklerCoroutine(
        List<Ball> balls,
        Dictionary<Ball, GameObject> spriteMap
    )
    {
        float elapsed        = 0f;
        float interval       = 1f / (spawnsPerSecond*Singleton.Instance.playerStats.GetWeaponPowerMult());
        float timeSinceSpawn = 0f;
        float angleOffset    = 0f;

        while (elapsed < sprinklerDuration)
        {
            while (Singleton.Instance.pauseManager.isPaused)
            {
                yield return null;
            }
            
            // stop on fire release
            if (releaseOnFireUp)
            {
                if (Singleton.Instance.playerInputManager.weaponFireUp)
                {
                    if (elapsed < 0.2f)
                    {
                        Singleton.Instance.gameHintManager.QueueHintUntilBouncingDone("Looks like I need to hold the weapon button to get the full effect!");
                    }
                    break;
                }
            }

            // drop any pooled‑out balls
            balls.RemoveAll(b => !b.gameObject.activeInHierarchy);
            if (balls.Count == 0)
                break;

            float dt = Time.deltaTime;
            elapsed        += dt;
            timeSinceSpawn += dt;
            angleOffset    += angularSpeed * dt;

            // — rotate each sprinkler sprite —
            foreach (var go in spriteMap.Values)
            {
                if (go.activeInHierarchy)
                    go.transform.Rotate(0, 0, angularSpeed * dt);
            }

            // — spawn when enough time has passed —
            if (timeSinceSpawn >= interval)
            {
                float angleStep = 360f / numberObjectsPerSpawn;
                foreach (var ball in balls)
                {
                    Vector2 center = ball.transform.position;
                    for (int i = 0; i < numberObjectsPerSpawn; i++)
                    {
                        float angleDeg = angleOffset + i * angleStep;
                        Vector2 dir = Helpers.AngleDegToVector2(angleDeg);

                        var proj = objectToSpawn.Spawn();
                        proj.transform.position = center;

                        if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                            rb.linearVelocity = dir * fireSpeed;
                    }
                }
                timeSinceSpawn -= interval;
            }

            yield return null;
        }

        // cleanup all remaining sprites
        foreach (var go in spriteMap.Values)
        {
            if (go != null)
            {
                go.transform.SetParent(null);
                go.SetActive(false);
            }
        }

        if (sprinklerSFXInstance.isValid())
        {
            sprinklerSFXInstance.stop(STOP_MODE.IMMEDIATE);
        }
    }
}
