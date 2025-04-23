// MenuPanel.cs
using UnityEngine;

public abstract class MenuPanel : MonoBehaviour
{
    /// <summary> Show (activate) this panel GameObject. </summary>
    public virtual void OnShow()
    {
        gameObject.SetActive(true);
    }

    /// <summary> Hide (deactivate) this panel GameObject. </summary>
    public virtual void OnHide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called after OnShow (and after any intro animations you kick off in OnShow).
    /// Use this to set your default selected button, etc.
    /// </summary>
    public virtual void OnFocus() { }
}