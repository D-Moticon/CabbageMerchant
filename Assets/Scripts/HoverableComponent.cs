using UnityEngine;

public class HoverableComponent : MonoBehaviour, IHoverable
{
    public string title;
    [TextArea]
    public string description;
    public string type;
    public string trigger;
    public string rarity;
    public string value;
    public Sprite image;


    public string GetTitleText(HoverableModifier hoverableModifier = null)
    {
        return title;
    }

    public string GetDescriptionText(HoverableModifier hoverableModifier = null)
    {
        return description;
    }

    public string GetTypeText(HoverableModifier hoverableModifier = null)
    {
        return type;
    }

    public string GetRarityText()
    {
        return rarity;
    }

    public string GetTriggerText()
    {
        return trigger;
    }

    public Sprite GetImage()
    {
        return image;
    }

    public string GetValueText()
    {
        return value;
    }
}
