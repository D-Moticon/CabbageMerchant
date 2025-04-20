using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayDialogue : MonoBehaviour
{
    public DialogueBox dialogueBox;

    [System.Serializable]
    public class DialogueInfo
    {
        public Dialogue dialogue;
        public bool isEnabled = true;
    }

    [System.Serializable]
    public class BiomeDialogueInfo
    {
        public Biome biome;
        public List<DialogueInfo> dialoguesToChooseFrom;
    }
    
    [Tooltip("Only enabled DialogueInfos will be considered for playback.")]
    public List<BiomeDialogueInfo> dialoguesToChooseFrom;

    [Header("Testing")]
    [Tooltip("If true, ignore the current biome filter and consider all enabled dialogues.")]
    public bool ignoreBiomeForTesting = false;
    
    void Start()
    {
        dialogueBox.HideDialogueBox();
        DialogueContext dc = new DialogueContext { dialogueBox = dialogueBox };
        Task t = new Task(PlayDialogueTask(dc));
    }

    private IEnumerator PlayDialogueTask(DialogueContext dc)
    {
        // figure out the list of enabled dialogues
        Biome currentBiome = Singleton.Instance.runManager.currentBiome;
        List<DialogueInfo> enabledList;

        if (ignoreBiomeForTesting || currentBiome == null)
        {
            // ignoreBiome OR no biome → use all enabled dialogues
            enabledList = dialoguesToChooseFrom
                .SelectMany(b => b.dialoguesToChooseFrom)
                .Where(info => info.isEnabled)
                .ToList();
        }
        else
        {
            // only the dialogues for this specific biome
            var biomeInfo = dialoguesToChooseFrom
                .FirstOrDefault(b => b.biome == currentBiome);

            enabledList = (biomeInfo != null)
                ? biomeInfo.dialoguesToChooseFrom
                    .Where(info => info.isEnabled)
                    .ToList()
                : new List<DialogueInfo>();
        }

        // nothing to play?
        if (enabledList.Count == 0)
        {
            Singleton.Instance.runManager.GoToMap();
            yield break;
        }

        // pick one at random
        int rand = Random.Range(0, enabledList.Count);
        var chosenInfo = enabledList[rand];

        // play it
        Task dialogueTask = new Task(chosenInfo.dialogue.PlayDialogue(dc));
        while (dialogueTask.Running)
            yield return null;

        // then go to map
        Singleton.Instance.runManager.GoToMap();
    }
}
