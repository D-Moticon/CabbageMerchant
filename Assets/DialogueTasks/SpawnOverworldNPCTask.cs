using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SpawnOverworldNPCTask : DialogueTask
{
    public OverworldNPC overworldNPCPrefab;
    public DialogueCharacter dialogueCharacter;
    public Vector2 spawnPos;
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        OverworldNPC npc = GameObject.Instantiate(overworldNPCPrefab, spawnPos, Quaternion.identity);
        npc.InitializeFromDialogueCharacter(dialogueCharacter);
        dc.npcA = npc;
        yield break;
    }
}
