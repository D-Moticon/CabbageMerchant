using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RunManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string startingSceneName = "MainMenuScene";  // The scene you want loaded at startup
    public string gameSceneName     = "GameScene";
    public string shopSceneName     = "ShopScene";

    [Header("Parent Object Name")]
    public string parentObjectName = "SceneParent";

    [Header("Transition Settings")]
    public float spawnOffsetX    = 20f;
    public float transitionTime  = 0.7f;

    private string currentSceneName;
    private GameObject currentSceneParent;

    void Start()
    {
        // Load the starting scene if none is loaded yet
        if (string.IsNullOrEmpty(currentSceneName) && !string.IsNullOrEmpty(startingSceneName))
        {
            // Slide to the starting scene
            StartCoroutine(SlideToScene(startingSceneName));
        }
    }

    public void GoToGame()
    {
        StartCoroutine(SlideToScene(gameSceneName));
    }

    public void GoToShop()
    {
        StartCoroutine(SlideToScene(shopSceneName));
    }

    /// <summary>
    /// Loads the target scene additively, finds its SceneParent,
    /// spawns it to the right, then slides old scene left + new scene in from the right.
    /// Finally, unloads the old scene.
    /// </summary>
    private IEnumerator SlideToScene(string newSceneName)
    {
        // If we're already in this scene, do nothing
        if (newSceneName == currentSceneName)
            yield break;

        // Load the new scene additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        // Get the loaded scene
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        if (!newScene.IsValid())
        {
            Debug.LogError($"Scene '{newSceneName}' not valid or not found. Check name/spelling.");
            yield break;
        }

        // Find the new scene's parent
        GameObject newSceneParent = null;
        foreach (GameObject root in newScene.GetRootGameObjects())
        {
            if (root.name == parentObjectName)
            {
                newSceneParent = root;
                break;
            }
        }

        if (newSceneParent == null)
        {
            Debug.LogWarning($"No object named '{parentObjectName}' found in '{newSceneName}'. " +
                             "Will not animate new scene in.");
            // If no parent, just unload old scene, set references, and return
            yield return UnloadOldScene();
            currentSceneName = newSceneName;
            currentSceneParent = null;
            yield break;
        }

        // We have an old scene parent only if we've already loaded something before
        Vector3 oldParentStartPos = Vector3.zero;
        Vector3 oldParentEndPos   = Vector3.zero;

        if (currentSceneParent != null)
        {
            // The old parent's start is its current position
            oldParentStartPos = currentSceneParent.transform.position;
            // Slide it left by spawnOffsetX
            oldParentEndPos = oldParentStartPos - new Vector3(spawnOffsetX, 0, 0);
        }

        // For the new scene parent, we place it to the right of the old parent's position
        Vector3 newParentOriginalPos = (currentSceneParent != null)
            ? currentSceneParent.transform.position
            : newSceneParent.transform.position; // If no old scene, keep new scene's original pos

        Vector3 newParentStartPos = new Vector3(newParentOriginalPos.x + spawnOffsetX,
                                               newParentOriginalPos.y,
                                               newParentOriginalPos.z);

        Vector3 newParentEndPos   = newParentOriginalPos;

        // Move the new scene parent to the start pos
        newSceneParent.transform.position = newParentStartPos;

        // Now we animate both over transitionTime
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            // Lerp old scene left, if it exists
            if (currentSceneParent != null)
            {
                currentSceneParent.transform.position = Vector3.Lerp(oldParentStartPos, oldParentEndPos, t);
            }

            // Lerp new scene in from the right
            newSceneParent.transform.position = Vector3.Lerp(newParentStartPos, newParentEndPos, t);

            yield return null;
        }

        // Final positions
        if (currentSceneParent != null)
            currentSceneParent.transform.position = oldParentEndPos;

        newSceneParent.transform.position = newParentEndPos;

        // Unload the old scene
        yield return UnloadOldScene();

        // Update references to new scene
        currentSceneName   = newSceneName;
        currentSceneParent = newSceneParent;
    }

    private IEnumerator UnloadOldScene()
    {
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            yield return SceneManager.UnloadSceneAsync(currentSceneName);
            currentSceneName = null;
            currentSceneParent = null;
        }
    }
}
