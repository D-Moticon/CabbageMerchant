using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AppearanceShopPanel : MenuPanel
{
    [Tooltip("Parent RectTransform for button grid")]
    public RectTransform gridParent;

    [Tooltip("Prefab for SkinButton")]
    public SkinButton skinButtonPrefab;

    [Tooltip("Database of all skins")]
    public SkinDatabase skinDatabase;

    private List<SkinButton> buttons = new List<SkinButton>();

    void OnEnable()
    {
        PopulateGrid();
    }

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

            // remember its demo-only flag
            btn.notInDemo = !skinInfo.InDemo;

            btn.Start(); // initialize UI

            // if we're in demo mode and this skin is restricted, gray it out
            if (Singleton.Instance.buildManager.IsDemoMode() && !skinInfo.InDemo)
                btn.SetToDemoRestricted();

            buttons.Add(btn);
        }
    }

    void Update()
    {
        foreach (var btn in buttons)
            btn.UpdateState();
    }
}