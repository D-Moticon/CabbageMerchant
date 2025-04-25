using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to any UI Button to show a specified MenuPanel via the MenuManager.
/// </summary>
[RequireComponent(typeof(Button))]
public class ShowPanelButton : MonoBehaviour
{
    [Tooltip("Name of the panel GameObject to show (must match the MenuPanel's GameObject name)")]
    public string panelName;

    [Tooltip("If true, this panel will be pushed onto the back‑stack (allowing Back() to return to it)")]
    public bool pushHistory = true;

    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnShowClicked);
    }

    void OnDestroy()
    {
        _button.onClick.RemoveListener(OnShowClicked);
    }

    private void OnMouseDown()
    {
        if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogWarning("ShowPanelButton: panelName is empty. Please set the panel name in the inspector.");
            return;
        }

        // Delegate to MenuManager to show the requested panel
        Singleton.Instance.menuManager.ShowPanel(panelName, pushHistory);
    }

    private void OnShowClicked()
    {
        /*if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogWarning("ShowPanelButton: panelName is empty. Please set the panel name in the inspector.");
            return;
        }

        // Delegate to MenuManager to show the requested panel
        Singleton.Instance.menuManager.ShowPanel(panelName, pushHistory);*/
    }
}