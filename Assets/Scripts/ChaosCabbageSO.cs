using UnityEngine;

[CreateAssetMenu(fileName = "ChaosCabbageSO", menuName = "Scriptable Objects/ChaosCabbage")]
public class ChaosCabbageSO : ScriptableObject
{
    public string displayName;
    public string dataName;
    public Color color = Color.white;
    public PetDefinition petDef;
    public Item item;

    public Dialogue cabbageGetDialogue;
    public bool InDemo = false;
}
