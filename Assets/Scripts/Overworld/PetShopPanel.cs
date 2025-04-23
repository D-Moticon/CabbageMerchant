using UnityEngine;
using System.Collections.Generic;

public class PetShopPanel : MenuPanel
{
    [Tooltip("Parent RectTransform for button grid")] public RectTransform gridParent;
    [Tooltip("Prefab for PetButton")] public PetButton petButtonPrefab;
    [Tooltip("ScriptableObject containing all PetDefinitions")] public PetDatabase petDatabase;

    private List<PetButton> buttons = new List<PetButton>();

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
        foreach (var def in petDatabase.allPets)
        {
            var btn = Instantiate(petButtonPrefab, gridParent);
            btn.def = def;
            btn.name = def.name + "Button";
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

