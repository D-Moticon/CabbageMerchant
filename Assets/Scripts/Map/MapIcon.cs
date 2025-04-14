using System;
using UnityEngine;
using MoreMountains.Feedbacks;

public class MapIcon : MonoBehaviour, IHoverable
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D bc2d;

    // If you want an easy way to store the scene reference on the icon:
    [HideInInspector] public MapPoint mapPoint;

    public SFXInfo hoverSFX;
    public MMF_Player hoverFeel;

    private void OnMouseDown()
    {
        MapSingleton.Instance.mapManager.OnMapIconClicked(this);
    }

    public string GetTitleText(HoverableModifier hoverableModifier = null)
    {
        hoverSFX.Play();
        hoverFeel.PlayFeedbacks();
        return mapPoint.displayName;
    }

    public string GetDescriptionText(HoverableModifier hoverableModifier = null)
    {
        return mapPoint.description;
    }

    public string GetRarityText()
    {
        return "";
    }

    public string GetTriggerText()
    {
        return "";
    }

    public Sprite GetImage()
    {
        return mapPoint.mapIcon;
    }

    public string GetValueText()
    {
        return "";
    }
}