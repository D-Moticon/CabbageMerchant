using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages global pause state. Called by the main Singleton to pause/unpause.
/// Supports a one-frame delay on unpause to absorb input events.
/// </summary>
public class PauseManager : MonoBehaviour
{
    /// <summary>
    /// True if the game is currently paused.
    /// </summary>
    public bool isPaused;

    /// <summary>
    /// Fired whenever pause state changes: true=paused, false=unpaused.
    /// </summary>
    public event Action<bool> OnPauseStateChanged;

    /// <summary>
    /// Pause or unpause the game. Unpause is delayed by one frame to avoid input bleed-through.
    /// </summary>
    public void SetPaused(bool paused)
    {
        if (isPaused == paused)
            return;

        if (!paused)
        {
            // Delay unpause by one frame
            StartCoroutine(UnpauseNextFrame());
        }
        else
        {
            // Immediate pause
            isPaused = true;
            OnPauseStateChanged?.Invoke(true);
        }
    }

    /// <summary>
    /// Toggle the pause state (honors one-frame delay on unpause).
    /// </summary>
    public void TogglePause() => SetPaused(!isPaused);

    public static bool IsPaused()
    {
        return Singleton.Instance.pauseManager.isPaused;
    }
    
    private IEnumerator UnpauseNextFrame()
    {
        // Wait exactly one frame
        yield return null;
        isPaused = false;
        OnPauseStateChanged?.Invoke(false);
    }
}