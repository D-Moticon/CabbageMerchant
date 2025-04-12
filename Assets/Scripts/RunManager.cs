using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Serialization;

public class RunManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string startingSceneName = "MainMenuScene";  
    public string gameSceneName     = "GameScene";
    public string shopSceneName     = "ShopScene";
    public string mapSceneName      = "Map";

    [Header("Parent Object Name")]
    public string parentObjectName = "SceneParent";

    [Header("Transition Settings")]
    [Tooltip("How far in world units to slide the new scene downward from the old scene's position.")]
    public float spawnOffsetY = 20f;
    public float transitionTime  = 0.7f;

    private string currentSceneName;
    private GameObject currentSceneParent;

    private Scene mapScene;
    private GameObject mapSceneParent;

    private int totalEncounters;
    [HideInInspector] public int currentmapLayer;

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

    public void GoToMap()
    {
        // Because we never unload the map scene, we skip re-loading if already loaded
        StartCoroutine(SlideToMapScene());
    }

    public void GoToScene(string sceneName)
    {
        StartCoroutine(SlideToScene(sceneName));
    }

    /// <summary>
    /// Loads a normal scene (Game, Shop, etc.) additively,
    /// slides the old scene downward (unless it's the map, in which case we just hide it),
    /// then unloads the old scene if it's not the map.
    /// </summary>
    private IEnumerator SlideToScene(string newSceneName)
    {
        // If we're already in this scene, do nothing
        if (newSceneName == currentSceneName)
            yield break;

        // Special check: if the old scene is the map, we won't unload it - just hide
        bool oldSceneIsMap = (currentSceneName == mapSceneName);

        // 1) Load the new scene additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        // 2) Find the new scene + parent
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        if (!newScene.IsValid())
        {
            Debug.LogError($"Scene '{newSceneName}' not valid or not found. Check name/spelling.");
            yield break;
        }

        GameObject newSceneParent = FindSceneParent(newScene);
        if (newSceneParent == null)
        {
            Debug.LogWarning($"No '{parentObjectName}' found in '{newSceneName}'. Will not animate.");
            yield return HideOrUnloadOldScene(oldSceneIsMap);
            currentSceneName   = newSceneName;
            currentSceneParent = null;
            yield break;
        }

        // 3) Set up positions for vertical sliding
        Vector3 oldParentStartPos = Vector3.zero;
        Vector3 oldParentEndPos   = Vector3.zero;

        if (currentSceneParent != null)
        {
            // Move old scene downward by spawnOffsetY
            oldParentStartPos = currentSceneParent.transform.position;
            oldParentEndPos   = oldParentStartPos - new Vector3(0, spawnOffsetY, 0);
        }

        // The new scene spawns above by spawnOffsetY
        Vector3 newParentOriginalPos = (currentSceneParent != null)
            ? currentSceneParent.transform.position
            : newSceneParent.transform.position;

        Vector3 newParentStartPos = new Vector3(
            newParentOriginalPos.x,
            newParentOriginalPos.y + spawnOffsetY,
            newParentOriginalPos.z
        );

        Vector3 newParentEndPos = newParentOriginalPos;

        // Move the new scene's parent up to start
        newSceneParent.transform.position = newParentStartPos;

        // 4) Animate
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            // Slide old scene downward
            if (currentSceneParent != null)
            {
                currentSceneParent.transform.position = Vector3.Lerp(
                    oldParentStartPos, oldParentEndPos, t
                );
            }

            // Slide new scene downward
            newSceneParent.transform.position = Vector3.Lerp(
                newParentStartPos, newParentEndPos, t
            );

            yield return null;
        }

        // 5) Final positions
        if (currentSceneParent != null)
            currentSceneParent.transform.position = oldParentEndPos;
        newSceneParent.transform.position = newParentEndPos;

        // 6) Hide or unload the old scene
        yield return HideOrUnloadOldScene(oldSceneIsMap);

        // 7) Update references
        currentSceneName   = newSceneName;
        currentSceneParent = newSceneParent;
    }

    /// <summary>
    /// Goes to the map scene. If not loaded yet, we load it once. Otherwise we just re-activate it.
    /// Then we do the same vertical slide from the current to the map scene,
    /// never unloading the map so it retains state.
    /// </summary>
    private IEnumerator SlideToMapScene()
    {
        // If map is already the current scene, do nothing
        if (currentSceneName == mapSceneName)
            yield break;

        // If the map scene isn't loaded yet, load it now
        if (!mapScene.IsValid())
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone)
                yield return null;

            // Cache references
            mapScene = SceneManager.GetSceneByName(mapSceneName);
            if (!mapScene.IsValid())
            {
                Debug.LogError($"Map scene '{mapSceneName}' failed to load!");
                yield break;
            }
            mapSceneParent = FindSceneParent(mapScene);
            if (mapSceneParent == null)
            {
                Debug.LogWarning($"No '{parentObjectName}' found in map scene. Can't animate it.");
            }
            else
            {
                // Hide initially
                mapSceneParent.SetActive(false);
            }
        }

        bool oldSceneIsMap = false;
        GameObject newSceneParent = mapSceneParent;

        if (newSceneParent == null)
        {
            Debug.LogWarning("Map scene has no parent object. Skipping animation...");
            yield return HideOrUnloadOldScene(false); 
            currentSceneName   = mapSceneName;
            currentSceneParent = null;
            yield break;
        }

        // Show the map scene parent
        newSceneParent.SetActive(true);

        // Slide old scene downward
        Vector3 oldParentStartPos = Vector3.zero;
        Vector3 oldParentEndPos   = Vector3.zero;

        if (currentSceneParent != null)
        {
            oldParentStartPos = currentSceneParent.transform.position;
            oldParentEndPos   = oldParentStartPos - new Vector3(0, spawnOffsetY, 0);
        }

        // Position the map scene up by spawnOffsetY
        Vector3 newParentOriginalPos = (currentSceneParent != null)
            ? currentSceneParent.transform.position
            : newSceneParent.transform.position;

        Vector3 newParentStartPos = new Vector3(
            newParentOriginalPos.x,
            newParentOriginalPos.y + spawnOffsetY,
            newParentOriginalPos.z
        );

        Vector3 newParentEndPos = newParentOriginalPos;
        newSceneParent.transform.position = newParentStartPos;

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            if (currentSceneParent != null)
            {
                currentSceneParent.transform.position = Vector3.Lerp(
                    oldParentStartPos, oldParentEndPos, t
                );
            }

            newSceneParent.transform.position = Vector3.Lerp(
                newParentStartPos, newParentEndPos, t
            );

            yield return null;
        }

        // Final
        if (currentSceneParent != null)
            currentSceneParent.transform.position = oldParentEndPos;
        newSceneParent.transform.position = newParentEndPos;

        // Hide/unload old scene (we never unload if it was the map)
        bool oldSceneWasMap = (currentSceneName == mapSceneName);
        yield return HideOrUnloadOldScene(oldSceneWasMap);

        // Now the map is current
        currentSceneName   = mapSceneName;
        currentSceneParent = newSceneParent;

        // Finally, call your code
        MapSingleton.Instance.mapManager.MoveToNextLayer();
    }

    /// <summary>
    /// If the old scene is the map, we just hide it. Otherwise, we unload normally.
    /// </summary>
    private IEnumerator HideOrUnloadOldScene(bool oldSceneIsMap)
    {
        if (string.IsNullOrEmpty(currentSceneName))
            yield break;

        if (oldSceneIsMap && mapSceneParent != null)
        {
            // Hide map parent instead of unloading
            mapSceneParent.SetActive(false);
        }
        else
        {
            // Normal unload
            yield return SceneManager.UnloadSceneAsync(currentSceneName);
        }

        currentSceneName   = null;
        currentSceneParent = null;
    }

    private GameObject FindSceneParent(Scene scene)
    {
        if (!scene.IsValid()) return null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == parentObjectName)
            {
                return root;
            }
        }
        return null;
    }
}
