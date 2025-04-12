using UnityEngine;
using System.Collections;

public class MapManager : MonoBehaviour
{
    [Header("References")]
    public MapBlueprint defaultMapBlueprint;

    [HideInInspector] public MapBlueprint currentMapBlueprint;
    [HideInInspector]public Map map;                      // The generated map we want to manage
    // Must match your Map script if you’re spacing each layer by 3.0f in Y

    [Header("Appearance")]
    public Color normalColor  = Color.white;
    public Color disabledColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Scrolling Settings")]
    public float scrollDuration = 0.5f; // how long to animate the scroll

    public int currentLayerIndex = 0;
    private bool isScrolling = false;

    private void Start()
    {
        map = MapSingleton.Instance.mapGenerator.GenerateMap(defaultMapBlueprint);
        currentMapBlueprint = defaultMapBlueprint;
        
        // Start at layer 0 (bottom)
        currentLayerIndex = 0;
        // Ensure icons reflect correct interactivity
        UpdateLayerStates();
        // Optionally center the map on layer 0
        CenterOnLayer(0, instant:true);
        
    }

    /// <summary>
    /// Called when the player is ready to move to the next layer of the map.
    /// This increments currentLayerIndex (unless we’re at the last),
    /// then smoothly scrolls the map and updates icon states.
    /// </summary>
    public void MoveToNextLayer()
    {
        if (isScrolling) return; // ignore if we’re mid-scroll
        if (currentLayerIndex >= map.layers.Count - 1)
        {
            Debug.Log("Already at the top layer; can't move further.");
            return;
        }

        currentLayerIndex++;
        UpdateLayerStates();
        StartCoroutine(ScrollToLayer(currentLayerIndex));
    }

    /// <summary>
    /// Called by MapIcons (or you can wire it differently) when an icon is clicked.
    /// If it's in the current layer, go to that scene.
    /// </summary>
    public void OnMapIconClicked(MapIcon icon)
    {
        // Find which layer this icon is in
        int layerIndex = FindIconLayer(icon);
        if (layerIndex == currentLayerIndex)
        {
            // Valid selection
            if (!string.IsNullOrEmpty(icon.sceneName))
            {
                // Call your runManager to load the scene
                Singleton.Instance.runManager.GoToScene(icon.sceneName);
            }
        }
        else
        {
            Debug.Log($"Icon {icon.name} is not in the current layer. Ignoring click.");
        }
    }

    /// <summary>
    /// Disables icons (collider + color) in all layers except the current one.
    /// </summary>
    private void UpdateLayerStates()
    {
        for (int i = 0; i < map.layers.Count; i++)
        {
            bool isCurrent = (i == currentLayerIndex);
            var layer = map.layers[i];

            foreach (var icon in layer.mapIcons)
            {
                // Gray out or restore color
                icon.spriteRenderer.color = isCurrent ? normalColor : disabledColor;

                // Enable/disable collisions
                if (icon.bc2d != null)
                {
                    icon.bc2d.enabled = isCurrent;
                }
            }
        }
    }

    /// <summary>
    /// Immediately snaps the map so that layerIndex is centered, ignoring animation.
    /// </summary>
    private void CenterOnLayer(int layerIndex, bool instant = false)
    {
        float targetY = -layerIndex * map.verticalSpacing;

        if (instant)
        {
            map.transform.localPosition = new Vector3(map.transform.localPosition.x, targetY, 0f);
        }
        else
        {
            // If you want to do a quick tween you can call StartCoroutine(...) here
        }
    }

    /// <summary>
    /// Smoothly scrolls the map to center the given layer.
    /// If each layer is placed at y = (layerIndex * layerVerticalSpacing),
    /// then we want the map transform to move to y = -(layerIndex * layerVerticalSpacing).
    /// </summary>
    private IEnumerator ScrollToLayer(int layerIndex)
    {
        isScrolling = true;

        Vector3 startPos = map.transform.localPosition;
        Vector3 endPos   = new Vector3(startPos.x, -layerIndex * map.verticalSpacing, 0f);

        float elapsed = 0f;
        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scrollDuration);

            map.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        map.transform.localPosition = endPos;

        isScrolling = false;
    }

    /// <summary>
    /// Finds which layer an icon belongs to, or returns -1 if not found.
    /// </summary>
    private int FindIconLayer(MapIcon icon)
    {
        for (int i = 0; i < map.layers.Count; i++)
        {
            if (map.layers[i].mapIcons.Contains(icon))
                return i;
        }
        return -1;
    }

    public void SetCurrentLayerIndex(int newLayer)
    {
        currentLayerIndex = newLayer;
    }
}
