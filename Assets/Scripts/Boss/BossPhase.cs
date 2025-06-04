using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossPhase
{
    [System.Serializable]
    public class DifficultyInfo
    {
        public Difficulty difficulty;
        public double totalHealth;
    }
    
    public List<DifficultyInfo> difficultyInfos;
    
    [Header("Board Population")]
    public BoardPopulateInfo boardPopulateInfo;

    [Header("Phase Effects")]
    [SerializeReference]
    public List<DialogueTask> preBoardPopulateTasks;
    [SerializeReference]
    public List<DialogueTask> postBoardPopulateTasks;
    [SerializeReference]
    public List<DialogueTask> postBounceStateExitedTasks;
}