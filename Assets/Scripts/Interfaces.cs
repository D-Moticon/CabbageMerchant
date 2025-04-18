using UnityEngine;

public interface IHoverable
{
    public string GetTitleText(HoverableModifier hoverableModifier = null);
    public string GetDescriptionText(HoverableModifier hoverableModifier = null);
    public string GetTypeText(HoverableModifier hoverableModifier = null);
    public string GetRarityText();
    public string GetTriggerText();
    public Sprite GetImage();
    public string GetValueText();
}
