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

public interface IBonkable
{
    public void Bonk(BonkParams bp);
    public void Remove();
    public GameObject GetGameObject();
}

public class BonkParams
{
    public float bonkerPower;
    public Vector2 collisionPos;
    public Vector2 normal = default;
    public Ball ball = null;
    public bool treatAsBall = false;
    public Cabbage bonkedCabbage;
    public IBonkable bonkable;
    public double totalBonkValueGained;
}

public interface IKillable
{
    public void Kill();
}

public interface IBiomeChangeable
{
    public void SetBiome(Biome biome);
}