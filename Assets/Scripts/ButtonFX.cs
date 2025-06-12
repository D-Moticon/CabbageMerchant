using System;
using UnityEngine;
using MoreMountains.Feedbacks;

public class ButtonFX : MonoBehaviour
{
    public MMF_Player hoverFeel;

    public SFXInfo hoverSFX;
    public SFXInfo clickSFX;
    
    private void OnMouseEnter()
    {
        hoverFeel.PlayFeedbacks();
        hoverSFX.Play();
    }

    private void OnMouseDown()
    {
        clickSFX.Play();
    }
}
