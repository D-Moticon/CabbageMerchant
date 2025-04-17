using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using MoreMountains.Feedbacks;

public class DialogueButton : MonoBehaviour
{
    public TMP_Text buttonText;
    [HideInInspector]public bool buttonPressed = false;
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
        buttonPressed = true;
        clickSFX.Play();
    }

    public void SetText(string text)
    {
        buttonText.text = text;
    }
}
