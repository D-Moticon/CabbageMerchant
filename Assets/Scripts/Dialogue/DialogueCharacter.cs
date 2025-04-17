using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "Dialogue/DialogueCharacter")]
public class DialogueCharacter : ScriptableObject
{
    public string displayName;
    public Sprite sprite;
    public SFXInfo speakingSFX_Short;
    public SFXInfo speakingSFX_Med;
    public SFXInfo speakingSFX_Long;
}
