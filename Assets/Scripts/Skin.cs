using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Skin", menuName = "Scriptable Objects/Skin")]
public class Skin : ScriptableObject
{
    public string displayName;
    public string dataName;
    [TextArea]
    public string description;
    public bool unlockedByDefault = false;
    public int cost = 25;
    [SerializeReference]
    public List<Requirement> requirements = new List<Requirement>();
    public Sprite dialogueSprite;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
}
