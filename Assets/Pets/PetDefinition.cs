using UnityEngine;

[CreateAssetMenu(fileName = "PetDefinition", menuName = "Pets/PetDefinition")]
public class PetDefinition : ScriptableObject
{
    public string displayName;
    public Item itemPrefab;
    public int cost;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
}
