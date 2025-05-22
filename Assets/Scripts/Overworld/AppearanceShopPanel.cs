using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AppearanceShopPanel : MenuPanel
{
    [Tooltip("Parent RectTransform for button grid")] public RectTransform gridParent;
    [Tooltip("Prefab for PetButton")] public SkinButton skinButtonPrefab;
    [Tooltip("ScriptableObject containing all PetDefinitions")] public SkinDatabase skinDatabase;

    private List<SkinButton> buttons = new List<SkinButton>();

    void OnEnable()
    {
        PopulateGrid();
    }

    /// <summary>
    /// Creates a PetButton for each definition in the database.
    /// </summary>
    public void PopulateGrid()
    {
        // clear old
        foreach (var btn in buttons)
            Destroy(btn.gameObject);
        buttons.Clear();

        // spawn new
        foreach (var skinInfo in skinDatabase.skinInfos)
        {
            var btn = Instantiate(skinButtonPrefab, gridParent);
            btn.skin = skinInfo.skin;
            btn.name = skinInfo.skin.displayName + "Button";
            btn.Start(); // manually initialize UI
            buttons.Add(btn);
        }
    }

    void Update()
    {
        // refresh state for each button
        foreach (var btn in buttons)
            btn.UpdateState();
    }
}
