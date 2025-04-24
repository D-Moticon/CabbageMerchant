using UnityEngine;
using System.Collections;

/// <summary>
/// Deactivates the GameObject after a specified lifetime.
/// When enabled, starts a timer and deactivates itself when time is up.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeactivateAfterTime : MonoBehaviour
{
    [Tooltip("Time in seconds before deactivation")] 
    public float lifetime = 3f;

    private Coroutine deactivateRoutine;

    void OnEnable()
    {
        // start or restart the deactivation timer
        if (deactivateRoutine != null)
            StopCoroutine(deactivateRoutine);
        deactivateRoutine = StartCoroutine(DeactivateCoroutine());
    }

    void OnDisable()
    {
        // ensure coroutine is stopped if object disabled
        if (deactivateRoutine != null)
            StopCoroutine(deactivateRoutine);
    }

    private IEnumerator DeactivateCoroutine()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }
}