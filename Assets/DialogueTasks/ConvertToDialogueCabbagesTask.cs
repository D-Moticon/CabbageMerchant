using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConvertToDialogueCabbagesTask : DialogueTask
{
    [Tooltip("Pooled cabbage prefab to spawn in place of each replaced cabbage")]    
    public PooledObjectData replacerCabbage;
    
    public int quantity = 1;

    [System.Serializable]
    public class DifficultyInfo
    {
        public Difficulty difficulty;
        public int bonksToPop = 5;
    }

    public List<DifficultyInfo> difficultyInfos;
    
    public PooledObjectData replaceVFX;
    public SFXInfo replaceSFX;

    public float delayBetween = 0.25f;

    [System.Serializable]
    public class DialogueInfo
    {
        public Dialogue dialogue;
        public Sprite icon;
    }

    [Tooltip("List of dialogues and icons to assign (no repeats if possible)")]
    public List<DialogueInfo> dialogues;

    public override IEnumerator RunTask(DialogueContext dc)
    {
        // Gather all active cabbages in the scene (must have a Cabbage component)
        var allCabbages = GameSingleton.Instance.gameStateMachine.activeCabbages;
        var candidates = new List<Cabbage>(allCabbages);

        // Determine how many to convert (don't exceed available)
        int toConvert = Mathf.Min(quantity, candidates.Count);

        // Prepare a pool of dialogue infos to avoid repeats
        var dialoguePool = new List<DialogueInfo>(dialogues);

        for (int i = 0; i < toConvert; i++)
        {
            // Pick a random cabbage to replace
            int cIdx = Random.Range(0, candidates.Count);
            var oldCabbage = candidates[cIdx];
            candidates.RemoveAt(cIdx);

            // Pick a dialogue entry (refill pool if empty)
            if (dialoguePool.Count == 0)
                dialoguePool = new List<DialogueInfo>(dialogues);

            int dIdx = Random.Range(0, dialoguePool.Count);
            var info = dialoguePool[dIdx];
            dialoguePool.RemoveAt(dIdx);

            // Play replace VFX at old cabbage position
            if (replaceVFX != null)
                replaceVFX.Spawn(oldCabbage.transform.position);

            // Play replace SFX
            replaceSFX?.Play();

            // Spawn the new dialogue cabbage
            var newGo = replacerCabbage.Spawn(oldCabbage.transform.position);

            // Assign dialogue and icon on pop
            var dt = newGo.GetComponent<DialogueTaskOnCabbageBonk>();
            if (dt != null)
            {
                dt.SetDialogue(info.dialogue);
                dt.SetIcon(info.icon);
                int bonkCD = 5;
                foreach (var di in difficultyInfos)
                {
                    if (di.difficulty == Singleton.Instance.playerStats.currentDifficulty)
                    {
                        bonkCD = di.bonksToPop;
                        break;
                    }
                }
                dt.SetBonkCountdown(bonkCD);
            }

            // Destroy or deactivate the old cabbage
            oldCabbage.Remove();

            if (delayBetween > 0.01f)
            {
                yield return new WaitForSeconds(delayBetween);
            }
        }

        yield break;
    }
}
