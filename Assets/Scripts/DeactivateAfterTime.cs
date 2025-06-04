using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Deactivates the GameObject after a specified lifetime.
/// When enabled, starts a timer and deactivates itself when time is up.
/// </summary>

public class DeactivateAfterTime : MonoBehaviour
{
    [Tooltip("Time in seconds before deactivation")] 
    public float lifetime = 3f;
    private float lifetimeCountdown;
    
    private Coroutine deactivateRoutine;

    void OnEnable()
    {
        lifetimeCountdown = lifetime;
    }

    private void Update()
    {
        if (Singleton.Instance.pauseManager.isPaused)
        {
            return;
        }

        lifetimeCountdown -= Time.deltaTime;
        if (lifetimeCountdown <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

}