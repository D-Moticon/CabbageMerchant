using UnityEngine;

public interface IHoverable
{
    public string GetTitleText();
    public string GetDescriptionText();
    public string GetRarityText();
    public string GetTriggerText();
    public Sprite GetImage();
}
