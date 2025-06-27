using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BoardGameSquare", menuName = "BoardGame/BoardGameSquare")]
public class BoardGameSquareSO : ScriptableObject
{
    public Sprite icon;
    [SerializeReference]
    public List<DialogueTask> onLandedTasks;
    public Color frameColor = Color.white;
}
