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

    public int musicPhase = 0;
    
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
    [SerializeReference]
    public List<DialogueTask> postPhaseBeatEarlyTasks;
    [SerializeReference]
    public List<DialogueTask> postPhaseBeatTasks;
    
    [Header("Loop Control")]
    public bool repeatUntilManuallyBroken = false;
}