using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Dynamically populates a grid of ChaosCabbageButtons based on the configured collection.
/// </summary>
public class ChaosCabbageButtonGrid : MonoBehaviour
{
    [Tooltip("The collection of chaos cabbages to display.")]
    public ChaosCabbageCollection chaosCabbageCollection;

    [Tooltip("Prefab for each chaos cabbage button.")]
    public ChaosCabbageButton chaosCabbageButtonPrefab;

    // Keep track of instantiated buttons so we can clear them if needed (optional)
    private readonly List<ChaosCabbageButton> _buttons = new List<ChaosCabbageButton>();

    private void OnEnable()
    {
        PopulateGrid();
    }

    private void OnDisable()
    {
        ClearGrid();
    }

    /// <summary>
    /// Instantiates a button for each ChaosCabbageSO in the collection, sets its sprite and SO.
    /// </summary>
    private void PopulateGrid()
    {
        ClearGrid();

        if (chaosCabbageCollection == null || chaosCabbageButtonPrefab == null)
        {
            Debug.LogWarning("ChaosCabbageButtonGrid: Missing collection or prefab.");
            return;
        }

        foreach (var cabbageSO in chaosCabbageCollection.chaosCabbages)
        {
            if (cabbageSO == null)
                continue;

            // Instantiate as child of this grid
            var btn = Instantiate(chaosCabbageButtonPrefab, transform);
            btn.chaosCabbage = cabbageSO;

            // Configure the image sprite & color
            if (btn.image != null)
            {
                var icon = cabbageSO.item != null ? cabbageSO.item.icon : null;
                if (icon != null)
                {
                    btn.image.sprite = icon;
                    btn.image.color = cabbageSO.color;
                }
            }

            btn.ApplyInitialState();
            
            _buttons.Add(btn);
        }
    }

    /// <summary>
    /// Destroys all child ChaosCabbageButton objects and clears internal list.
    /// </summary>
    private void ClearGrid()
    {
        // Find any ChaosCabbageButton components under this transform and destroy their GameObjects
        var childButtons = GetComponentsInChildren<ChaosCabbageButton>(includeInactive: true);
        foreach (var btn in childButtons)
        {
            if (btn != null && btn.gameObject != null)
                Destroy(btn.gameObject);
        }

        // Clear our tracking list
        _buttons.Clear();
    }
}
