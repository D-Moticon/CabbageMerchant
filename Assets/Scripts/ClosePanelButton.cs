using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this to any UI Button (e.g. an 'X' close button) to automatically
/// find the enclosing MenuPanel and close it, returning to the previous panel.
/// </summary>
[RequireComponent(typeof(Button))]
public class ClosePanelButton : MonoBehaviour
{
    private Button _button;

    void Awake()
    {
        //_button = GetComponent<Button>();
        //_button.onClick.AddListener(OnCloseClicked);
    }

    void OnDestroy()
    {
        //_button.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnMouseDown()
    {
        // Find the nearest MenuPanel in parents
        var panel = GetComponentInParent<MenuPanel>();
        if (panel == null)
        {
            Debug.LogWarning("ClosePanelButton: no parent MenuPanel found to close.");
            return;
        }
        
        // Use the MenuManager to go back in history,
        // which hides the current panel and shows the previous one.
        Singleton.Instance.menuManager.Back();
    }
}