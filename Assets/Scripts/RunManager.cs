using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.AI;

public class RunManager : MonoBehaviour
{
    public MapBlueprint startingMapBlueprint;
    [Header("Scene Names")]

    public string runStartScene = "Map";
    public string mapSceneName      = "Map";

    [Header("Parent Object Name")]
    public string parentObjectName = "SceneParent";

    // New enum to choose transition direction
    public enum TransitionDirection { Top, Right }

    [Header("Transition Settings")]
    [Tooltip("Distance in world units to slide the new scene into view. " +
             "For a 'Top' transition, the scene slides vertically from above. " +
             "For a 'Right' transition, the scene slides horizontally from the right.")]
    public float spawnOffset = 20f;
    
    [Tooltip("Select the direction from which the new scene will slide in.")]
    public TransitionDirection transitionDirection = TransitionDirection.Top;
    
    public float transitionTime  = 0.7f;

    [HideInInspector]public string currentSceneName;
    private GameObject currentSceneParent;
    private Coroutine slideSceneRoutine = null;
    private bool isSliding = false;

    private Scene mapScene;
    private GameObject mapSceneParent;

    private int totalEncounters;
    [HideInInspector] public int currentmapLayer;
    public Biome startingBiome;
    public Biome currentBiome;
    [HideInInspector]public MapPoint currentMapPoint;

    public delegate void BiomeDelegate(Biome biome);

    public static event BiomeDelegate BiomeChangedEvent;
    
    public class RunStartParams
    {
        
    }

    public class RunCompleteParams
    {
        public bool success;
        public float runTime;
        public double totalBonkValue;
        public PetDefinition petDefinition;
        public Difficulty difficulty;
        public string customEndString;
    }
    
    public delegate void RunStartDelegate(RunStartParams rsp);

    public static event RunStartDelegate RunStartEvent;
    public static RunStartDelegate RunStartEventLate;
    
    public delegate void RunEndDelegate(RunCompleteParams rep);
    public static event RunEndDelegate RunFinishedEvent;
    public static System.Action RunEndedEvent;
    public static System.Action RunStartEventNoParams;
    
    public delegate void StringDelegate(string s);
    public static StringDelegate SceneChangedEvent;

    public void GoToMap()
    {
        print("Going to map");
        // Because we never unload the map scene, we skip re-loading if already loaded
        StartCoroutine(SlideToMapScene());
    }

    public void GoToScene(string sceneName, MapPoint mapPoint = null)
    {
        Debug.Log($"Going to scene {sceneName}");
        currentMapPoint = mapPoint;
        SceneChangedEvent?.Invoke(sceneName);
        
        //This branch is to prevent overlapping scene slides from causing unpredicatable behavior
        if (isSliding)
        {
            StartCoroutine(WaitThenSlide(sceneName, mapPoint));
        }
        else
        {
            slideSceneRoutine = StartCoroutine(SlideToScene(sceneName, mapPoint));
        }
    }

    private IEnumerator WaitThenSlide(string sceneName, MapPoint mapPoint)
    {
        while (isSliding)
            yield return null;

        slideSceneRoutine = StartCoroutine(SlideToScene(sceneName, mapPoint));
    }
    
    /// <summary>
    /// Loads a normal scene (Game, Shop, etc.) additively,
    /// slides the old scene out and slides the new scene into place,
    /// then unloads the old scene (unless it is the map).
    /// </summary>
    private IEnumerator SlideToScene(string newSceneName, MapPoint mapPoint = null)
    {
        // If we're already in this scene, do nothing
        if (newSceneName == currentSceneName)
            yield break;

        isSliding = true;
        
        // Special check: if the old scene is the map, we won't unload it - just hide it.
        bool oldSceneIsMap = (currentSceneName == mapSceneName);

        //Biome and Boss
        if (mapPoint != null)
        {
            if (mapPoint.biome != null)
            {
                if (mapPoint.biome != currentBiome && mapPoint.biome != null)
                {
                    ChangeBiome(mapPoint.biome);
                }
            }
            
            Singleton.Instance.bossFightManager.SetBossFight(mapPoint.boss);
        }
        
        //Special Rules
        if (mapPoint != null)
        {
            if (mapPoint.specialGameRules != null)
            {
                foreach (var sgr in mapPoint.specialGameRules)
                {
                    Singleton.Instance.specialRuleManager.AddSpecialGameRule(sgr);
                }
            }
            
            //Specific Dialogue
            Singleton.Instance.dialogueManager.SetNextSpecificDialogue(mapPoint.specificDialogue);
        }

        // 1) Load the new scene additively.
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        // 2) Find the new scene and its parent.
        Scene newScene = SceneManager.GetSceneByName(newSceneName);
        if (!newScene.IsValid())
        {
            Debug.LogError($"Scene '{newSceneName}' not valid or not found. Check name/spelling.");
            isSliding = false;
            yield break;
        }

        GameObject newSceneParent = FindSceneParent(newScene);

        if (newSceneParent == null)
            Debug.LogError($"[RunManager] FindSceneParent returned null for scene '{newSceneName}'. parentObjectName='{parentObjectName}'");
        
        NavMeshAgent[] navMeshAgents = newSceneParent.GetComponentsInChildren<NavMeshAgent>();
        foreach (var nma in navMeshAgents)
        {
            nma.enabled = false;
        }
        
        if (newSceneName == mapSceneName && newSceneParent != null)
        {
            mapScene       = newScene;
            mapSceneParent = newSceneParent;
            // and make sure it's active (in case you hid it previously)
            mapSceneParent.SetActive(true);
        }
        
        if (newSceneParent == null)
        {
            Debug.LogWarning($"No '{parentObjectName}' found in '{newSceneName}'. Will not animate.");
            yield return HideOrUnloadOldScene(oldSceneIsMap);
            currentSceneName   = newSceneName;
            currentSceneParent = null;
            isSliding = false;
            yield break;
        }

        // 3) Set up positions for sliding transition.
        Vector3 oldParentStartPos = Vector3.zero;
        Vector3 oldParentEndPos   = Vector3.zero;

        if (currentSceneParent != null)
        {
            oldParentStartPos = currentSceneParent.transform.position;
            if(transitionDirection == TransitionDirection.Top)
            {
                // Slide old scene downward (vertical slide).
                oldParentEndPos = oldParentStartPos - new Vector3(0, spawnOffset, 0);
            }
            else if(transitionDirection == TransitionDirection.Right)
            {
                // Slide old scene leftward (horizontal slide).
                oldParentEndPos = oldParentStartPos - new Vector3(spawnOffset, 0, 0);
            }
        }

        // Determine the original position for the new scene.
        Vector3 newParentOriginalPos = (currentSceneParent != null)
            ? currentSceneParent.transform.position
            : newSceneParent.transform.position;

        Vector3 newParentStartPos;
        if (transitionDirection == TransitionDirection.Top)
        {
            newParentStartPos = new Vector3(
                newParentOriginalPos.x,
                newParentOriginalPos.y + spawnOffset,
                newParentOriginalPos.z
            );
        }
        else // TransitionDirection.Right
        {
            newParentStartPos = new Vector3(
                newParentOriginalPos.x + spawnOffset,
                newParentOriginalPos.y,
                newParentOriginalPos.z
            );
        }

        Vector3 newParentEndPos = newParentOriginalPos;

        // Move the new scene's parent to the starting position.
        newSceneParent.transform.position = newParentStartPos;

        // 4) Animate the sliding transition.
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            // Slide old scene.
            if (currentSceneParent != null)
            {
                currentSceneParent.transform.position = Vector3.Lerp(oldParentStartPos, oldParentEndPos, t);
            }

            // Slide new scene.
            newSceneParent.transform.position = Vector3.Lerp(newParentStartPos, newParentEndPos, t);

            yield return null;
        }

        // 5) Ensure final positions are set.
        if (currentSceneParent != null)
            currentSceneParent.transform.position = oldParentEndPos;
        newSceneParent.transform.position = newParentEndPos;

        // 6) Hide or unload the old scene.
        yield return HideOrUnloadOldScene(oldSceneIsMap);

        // 7) Update references.
        currentSceneName   = newSceneName;
        currentSceneParent = newSceneParent;
        SceneManager.SetActiveScene(newScene);
        
        //NavMeshAgents
        foreach (var nma in navMeshAgents)
        {
            nma.enabled = true;
        }
        
        //Map Point Extras
        if (mapPoint != null)
        {
            if (mapPoint.mapPointExtras != null)
            {
                for (int i = 0; i < mapPoint.mapPointExtras.Count; i++)
                {
                    mapPoint.mapPointExtras[i].GenerateMapPointExtra();
                }
            }
        }

        
        // 8) Teleport the NavMeshAgent back to a known spawn point
        var walker = currentSceneParent.gameObject.GetComponentInChildren<OverworldCharacter>();
        if (walker != null)
        {
            var agent = walker.GetComponent<UnityEngine.AI.NavMeshAgent>();
            // If you have a dedicated PlayerStart object in your map:
            Transform startMarker = currentSceneParent.transform.Find("PlayerStart");
            Vector3 spawnPos = startMarker != null
                ? startMarker.position
                : walker.transform.position;  // fallback to wherever they currently are

            // Warp the agent (this instantly moves both the transform and internal navmesh state)
            agent.Warp(spawnPos);
            // Also force the transform in case Warp alone doesn’t
            walker.transform.position = spawnPos;
            // Clear any leftover path
            agent.ResetPath();
        }
        
        isSliding = false;
    }

    public void ChangeBiome(Biome newBiome)
    {
        BiomeChangedEvent?.Invoke(newBiome);
        currentBiome = newBiome;
    }
    
    /// <summary>
    /// Goes to the map scene. If not already loaded, it loads the scene once;
    /// otherwise, it just re-activates it. Then performs a slide transition 
    /// (using the selected direction) without unloading the map scene.
    /// </summary>
    private IEnumerator SlideToMapScene()
    {
        // If map is already the current scene, do nothing.
        if (currentSceneName == mapSceneName)
            yield break;

        // If the map scene isn’t loaded yet, load it now.
        if (!mapScene.IsValid())
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone)
                yield return null;

            // Cache references.
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
                // Initially hide the map's parent.
                mapSceneParent.SetActive(false);
            }
        }
        
        GameObject newSceneParent = mapSceneParent;

        if (newSceneParent == null)
        {
            Debug.LogWarning("Map scene has no parent object. Skipping animation...");
            yield return HideOrUnloadOldScene(false); 
            currentSceneName   = mapSceneName;
            currentSceneParent = null;
            yield break;
        }

        // Show the map scene.
        newSceneParent.SetActive(true);

        // Set up positions for the transition.
        Vector3 oldParentStartPos = Vector3.zero;
        Vector3 oldParentEndPos   = Vector3.zero;

        if (currentSceneParent != null)
        {
            oldParentStartPos = currentSceneParent.transform.position;
            if (transitionDirection == TransitionDirection.Top)
            {
                oldParentEndPos = oldParentStartPos - new Vector3(0, spawnOffset, 0);
            }
            else // TransitionDirection.Right
            {
                oldParentEndPos = oldParentStartPos - new Vector3(spawnOffset, 0, 0);
            }
        }

        Vector3 newParentOriginalPos = (currentSceneParent != null)
            ? currentSceneParent.transform.position
            : newSceneParent.transform.position;

        Vector3 newParentStartPos;
        if (transitionDirection == TransitionDirection.Top)
        {
            newParentStartPos = new Vector3(
                newParentOriginalPos.x,
                newParentOriginalPos.y + spawnOffset,
                newParentOriginalPos.z
            );
        }
        else // TransitionDirection.Right
        {
            newParentStartPos = new Vector3(
                newParentOriginalPos.x + spawnOffset,
                newParentOriginalPos.y,
                newParentOriginalPos.z
            );
        }
        Vector3 newParentEndPos = newParentOriginalPos;
        newSceneParent.transform.position = newParentStartPos;

        // Animate the transition.
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            if (currentSceneParent != null)
            {
                currentSceneParent.transform.position = Vector3.Lerp(oldParentStartPos, oldParentEndPos, t);
            }

            newSceneParent.transform.position = Vector3.Lerp(newParentStartPos, newParentEndPos, t);
            yield return null;
        }

        // Ensure final positions.
        if (currentSceneParent != null)
            currentSceneParent.transform.position = oldParentEndPos;
        newSceneParent.transform.position = newParentEndPos;

        // Hide or unload the old scene (we never unload if it was the map).
        bool oldSceneWasMap = (currentSceneName == mapSceneName);
        yield return HideOrUnloadOldScene(oldSceneWasMap);

        // Update current scene references.
        currentSceneName   = mapSceneName;
        currentSceneParent = newSceneParent;

        // Finally, perform any additional map actions.
        MapSingleton.Instance.mapManager.MoveToNextLayer();
        SceneManager.SetActiveScene(mapScene);
    }

    /// <summary>
    /// If the old scene is the map, we just hide it; otherwise, we unload it.
    /// </summary>
    private IEnumerator HideOrUnloadOldScene(bool oldSceneIsMap)
    {
        if (string.IsNullOrEmpty(currentSceneName))
            yield break;

        if (oldSceneIsMap && mapSceneParent != null)
        {
            // Hide the map scene's parent instead of unloading.
            mapSceneParent.SetActive(false);
        }
        else
        {
            // Unload the old scene normally.
            yield return SceneManager.UnloadSceneAsync(currentSceneName);
        }

        currentSceneName   = null;
        currentSceneParent = null;
    }

    /// <summary>
    /// Searches the scene for a GameObject with the specified parent name.
    /// </summary>
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
    
    public void StartNewRun(string startScene = "Map")
    {
        // Fire any listeners
        RunStartParams rsp = new RunStartParams();
        RunStartEvent?.Invoke(rsp);
        RunStartEventNoParams?.Invoke();
        
        ChangeBiome(startingBiome);
        GoToSceneExclusive(startScene);
        RunStartEventLate?.Invoke(rsp);
    }

    public void GoToSceneExclusive(string sceneName)
    {
        // 1) Figure out which scene is our GlobalScene (the one that remains loaded)
        string globalSceneName = "GlobalScene";

        // 2) Collect every other scene’s name so we can unload them
        int sceneCount = SceneManager.sceneCount;
        var toUnload = new List<string>(sceneCount);
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != globalSceneName)
                toUnload.Add(scene.name);
        }

        // 3) Unload them asynchronously
        foreach (var name in toUnload)
            SceneManager.UnloadSceneAsync(name);

        // 4) Reset our bookkeeping
        currentSceneName   = null;
        currentSceneParent = null;

        SceneChangedEvent?.Invoke(sceneName);
        
        // 5) Finally, slide in the starting scene
        if (!string.IsNullOrEmpty(sceneName))
            StartCoroutine(SlideToScene(sceneName));
    }
    
    public void ReloadCurrentScene()
    {
        if (string.IsNullOrEmpty(currentSceneName))
        {
            Debug.LogWarning("RunManager.ReloadCurrentScene: no scene to reload.");
            return;
        }
        StartCoroutine(ReloadSceneRoutine(currentSceneName));
    }

    private IEnumerator ReloadSceneRoutine(string sceneName)
    {
        yield return null; //Needed to let events fire to stop coroutines etc.
        
        // 1) Unload just that scene
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
        while (!unloadOp.isDone)
            yield return null;
        
        yield return null;
        
        // 2) Load it back additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
            yield return null;

        // 3) Update our references so future transitions still work
        currentSceneName = sceneName;
        var sc = SceneManager.GetSceneByName(sceneName);
        currentSceneParent = FindSceneParent(sc);
        if (currentSceneParent == null)
            Debug.LogWarning($"RunManager: Couldn't find '{parentObjectName}' in reloaded '{sceneName}'.");
        SceneManager.SetActiveScene(sc);
    }
    
    public void FinishRun(bool success, string customEndString = "")
    {
        //This displays the run complete screen
        GoToMap();
        Singleton.Instance.menuManager.ShowPanel("RunEnd");
        RunCompleteParams rcp = new RunCompleteParams();
        rcp.success = success;
        rcp.totalBonkValue = Singleton.Instance.playerStats.totalBonkValueThisRun;
        rcp.runTime = Singleton.Instance.playerStats.totalRunTime;

        if (Singleton.Instance.buildManager.IsDemoMode())
        {
            rcp.runTime = 9999999999;
        }
        
        rcp.petDefinition = Singleton.Instance.petManager.currentPet;
        rcp.difficulty = Singleton.Instance.playerStats.currentDifficulty;
        rcp.customEndString = customEndString;
        
        RunFinishedEvent?.Invoke(rcp);
    }

    public void EndRunToOverworld()
    {
        //This sends you back to the overworld, resets items, etc.
        RunEndedEvent?.Invoke();
        ChangeBiome(startingBiome);
        GoToSceneExclusive("Overworld");
    }

    IEnumerator EndRunRoutine()
    {
        yield break;
    }

    public void QuitGame()
    {
        Singleton.Instance.saveManager.SaveToDisk();
        Application.Quit();
    }

    
}
