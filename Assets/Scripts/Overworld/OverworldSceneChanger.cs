using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages switching between Overworld and building scenes.
/// Ensures only one of the configured scenes is loaded at a time.
/// GlobalScene remains always loaded.
/// </summary>
public class OverworldSceneChanger : MonoBehaviour
{
    [Tooltip("List of scene names this manager will handle (e.g., Overworld, Museum, Shop).")]
    public string[] managedScenes;

    // Pending info for scene load
    private string pendingSceneName;
    private Vector2? pendingCharacterPosition;
    private Vector2? pendingCharacterTargetDelta;

    /// <summary>
    /// Switches to the specified scene: unloads any other managed scenes then loads the target.
    /// Optionally moves the OverworldCharacter to the specified position and/or sets a walk target delta.
    /// </summary>
    public void ChangeScene(string newSceneName, Vector2? overworldCharacterPosition = null, Vector2? overworldCharacterTargetPosDelta = null)
    {
        if (string.IsNullOrEmpty(newSceneName))
            return;

        // Store pending actions
        pendingSceneName = newSceneName;
        if (overworldCharacterPosition != null)
        {
            pendingCharacterPosition = overworldCharacterPosition;
        }

        else
        {
            pendingCharacterPosition = Vector2.zero;
        }

        if (pendingCharacterTargetDelta != null)
        {
            pendingCharacterTargetDelta = overworldCharacterTargetPosDelta;
        }

        else
        {
            pendingCharacterTargetDelta = Vector2.zero;
        }

        print(pendingCharacterPosition);
        
        // Unload other managed scenes
        foreach (var name in managedScenes)
        {
            if (name != newSceneName && SceneManager.GetSceneByName(name).isLoaded)
                SceneManager.UnloadSceneAsync(name);
        }

        // Load target scene if not already
        var targetScene = SceneManager.GetSceneByName(newSceneName);
        if (!targetScene.isLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(newSceneName, LoadSceneMode.Additive);
        }
        else
        {
            // Already loaded: handle pending immediately
            OnSceneLoaded(targetScene, LoadSceneMode.Additive);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only handle our requested scene
        if (scene.name != pendingSceneName)
            return;

        // Unsubscribe to avoid duplicate calls
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Find the OverworldCharacter component in any active scene
        var walker = Object.FindObjectOfType<OverworldCharacter>();
        if (walker != null)
        {
            // Reposition if requested
            if (pendingCharacterPosition.HasValue)
            {
                walker.transform.position = pendingCharacterPosition.Value;
            }

            // Set a target delta if requested, with one-frame delay
            if (pendingCharacterTargetDelta.HasValue && pendingCharacterPosition.HasValue)
            {
                Vector2 targetPos = pendingCharacterPosition.Value + pendingCharacterTargetDelta.Value;
                StartCoroutine(DelayedSetTarget(walker, targetPos));
            }
        }

        // Clear pending
        pendingSceneName = null;
        pendingCharacterPosition = null;
        pendingCharacterTargetDelta = null;
    }

    private IEnumerator DelayedSetTarget(OverworldCharacter walker, Vector2 targetPos)
    {
        // Wait one frame
        yield return null;
        walker.SetTargetPosition(targetPos);
    }
}