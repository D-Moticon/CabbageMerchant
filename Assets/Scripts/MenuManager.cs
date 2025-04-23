// MenuManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    public string firstPanelName;
    [Tooltip("Drag in all of your MenuPanel components here")]
    [SerializeField] private List<MenuPanel> allPanels;

    // fast lookup by panel name
    private Dictionary<string, MenuPanel> panelsByName;
    // history stack for Back() behavior
    private Stack<MenuPanel> history = new Stack<MenuPanel>();

    void Awake()
    {
        panelsByName = allPanels.ToDictionary(p => p.gameObject.name, p => p);
        // ensure everything starts hidden
        foreach (var panel in allPanels)
            panel.OnHide();
        ShowPanel(firstPanelName);
    }

    /// <summary>
    /// Show a panel by its GameObject name.
    /// If pushHistory is true, it will be added to the back‑stack.
    /// </summary>
    public void ShowPanel(string panelName, bool pushHistory = true)
    {
        if (!panelsByName.TryGetValue(panelName, out var next))
            return;

        // hide current
        if (history.Count > 0)
            history.Peek().OnHide();

        // show next
        next.OnShow();
        next.OnFocus();

        if (pushHistory)
            history.Push(next);

        // pause the game any time at least one panel is open
        Singleton.Instance.pauseManager.SetPaused(true);
    }

    /// <summary>
    /// Go back to the previous panel, if any.
    /// </summary>
    public void Back()
    {
        if (history.Count == 0) return;

        // hide current
        var current = history.Pop();
        current.OnHide();

        if (history.Count == 0)
        {
            // no panels left → unpause
            Singleton.Instance.pauseManager.SetPaused(false);
            return;
        }

        // show previous
        var prev = history.Peek();
        prev.OnShow();
        prev.OnFocus();

        // still paused, because there's still at least one panel open
    }

    /// <summary>
    /// Hide all panels and clear the history stack.
    /// </summary>
    public void HideAll()
    {
        while (history.Count > 0)
        {
            var panel = history.Pop();
            panel.OnHide();
        }
        // clear everything → unpause
        Singleton.Instance.pauseManager.SetPaused(false);
    }
}