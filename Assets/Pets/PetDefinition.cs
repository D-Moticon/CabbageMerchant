using UnityEngine;

[CreateAssetMenu(fileName = "PetDefinition", menuName = "Pets/PetDefinition")]
public class PetDefinition : ScriptableObject
{
    public bool InDemo = false;
    public string displayName;
    public string dataName;
    public Item itemPrefab;
    public int cost;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
}
