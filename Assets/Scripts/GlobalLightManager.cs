using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Handles toggling of biome-specific global lights in the GlobalScene,
/// and disables them when a loaded scene provides its own global light.
/// Attach to a GameObject in GlobalScene containing the three biome lights as children.
/// </summary>
public class GlobalLightManager : MonoBehaviour
{
    [Tooltip("GlobalScene biome lights: index/order irrelevant.")]
    public Light2D[] biomeGlobalLights;

    void Awake()
    {
        // Ensure at least the biome lights are active if no other global lights loaded
        SceneManager.sceneLoaded += OnAnySceneLoaded;
        SyncBiomeLights();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnAnySceneLoaded;
    }

    private void OnAnySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip callback when GlobalScene loads
        if (scene == gameObject.scene)
            return;

        // Check if newly loaded scene has its own global Light2D
        bool sceneHasGlobal = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            var lights = root.GetComponentsInChildren<Light2D>(true);
            foreach (var l in lights)
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    sceneHasGlobal = true;
                    break;
                }
            }
            if (sceneHasGlobal)
                break;
        }

        // Toggle biome lights accordingly
        foreach (var bioLight in biomeGlobalLights)
        {
            if (bioLight != null)
                bioLight.enabled = !sceneHasGlobal;
        }
    }

    /// <summary>
    /// If no other non-global lights exist at startup, ensure biome lights are on.
    /// </summary>
    private void SyncBiomeLights()
    {
        // If multiple scenes already loaded, check them
        bool anyOtherGlobal = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene == gameObject.scene)
                continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                var lights = root.GetComponentsInChildren<Light2D>(true);
                foreach (var l in lights)
                {
                    if (l.lightType == Light2D.LightType.Global)
                    {
                        anyOtherGlobal = true;
                        break;
                    }
                }
                if (anyOtherGlobal)
                    break;
            }
            if (anyOtherGlobal)
                break;
        }

        // If no other scene global, enable biome lights
        if (!anyOtherGlobal)
        {
            foreach (var bioLight in biomeGlobalLights)
            {
                if (bioLight != null)
                    bioLight.enabled = true;
            }
        }
    }
}
