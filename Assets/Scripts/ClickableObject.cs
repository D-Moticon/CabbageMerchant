using System;
using UnityEngine;
using MoreMountains.Feedbacks;

public abstract class ClickableObject : MonoBehaviour
{
    public SFXInfo hoverSFX;
    public MMF_Player hoverFeel;
    public SFXInfo clickSFX;
    public SFXInfo failClickSFX;
    public FloaterReference failClickFloater;
    public MMF_Player clickFeel;

    public abstract void TryClick();

    private void OnMouseEnter()
    {
        hoverSFX.Play();
        if (hoverFeel != null)
        {
            hoverFeel.PlayFeedbacks();
        }
    }

    public virtual void Click()
    {
        clickSFX.Play();
        clickFeel.PlayFeedbacks();
    }

    public virtual void FailClick(string failClickText = null)
    {
        failClickSFX.Play();
        if (failClickText != null)
        {
            Singleton.Instance.floaterManager.SpawnFloater(failClickFloater, failClickText, this.transform.position, Color.red);
        }
        
    }
}
