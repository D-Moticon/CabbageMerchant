using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

[CreateAssetMenu(fileName = "Boss", menuName = "Scriptable Objects/Boss")]
public class Boss : ScriptableObject
{
    public string bossName;
    public Sprite bossSprite;
    public EventReference bossMusic;
    public EventReference bossAfterMusic;
    public List<BossPhase> phases = new List<BossPhase>();
    [SerializeReference]
    public List<DialogueTask> postBeatTasks;

}
