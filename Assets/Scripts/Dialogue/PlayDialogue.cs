using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class PlayDialogue : MonoBehaviour
{
    public DialogueBox dialogueBox;

    [System.Serializable]
    public class DialogueInfo
    {
        public Dialogue dialogue;
        public bool isEnabled = true;
        [Tooltip("If set to true, only this dialogue will be chosen (ignores others).")]
        public bool solo = false;
    }

    [System.Serializable]
    public class BiomeDialogueInfo
    {
        public Biome biome;

        [ListDrawerSettings(ShowFoldout = true, ElementColor = "GetDialogueInfoColor")]
        public List<DialogueInfo> dialoguesToChooseFrom;

        // Odin requires signature: Color MethodName(int index)
        private Color GetDialogueInfoColor(int index)
        {
            var info = dialoguesToChooseFrom[index];
            bool anySolo = dialoguesToChooseFrom.Any(d => d.solo);
            if (anySolo)
            {
                return info.solo ? Color.forestGreen : Color.gray;
            }
            else
            {
                return info.isEnabled ? Color.forestGreen : Color.gray;
            }
        }
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

        Dialogue specificDialogue = Singleton.Instance.dialogueManager.nextSpecificDialogue;
        if (specificDialogue != null)
        {
            Task specT = new Task(specificDialogue.PlayDialogue(dc));
            specificDialogue = null;
        }
        else
        {
            Task t = new Task(PlayDialogueTask(dc));
        }
        
    }

    private IEnumerator PlayDialogueTask(DialogueContext dc)
    {
        Biome currentBiome = Singleton.Instance.runManager.currentBiome;
        List<DialogueInfo> enabledList;

        if (ignoreBiomeForTesting || currentBiome == null)
        {
            enabledList = dialoguesToChooseFrom
                .SelectMany(b => b.dialoguesToChooseFrom)
                .Where(info => info.isEnabled)
                .ToList();
        }
        else
        {
            var biomeInfo = dialoguesToChooseFrom
                .FirstOrDefault(b => b.biome == currentBiome);

            enabledList = (biomeInfo != null)
                ? biomeInfo.dialoguesToChooseFrom.Where(info => info.isEnabled).ToList()
                : new List<DialogueInfo>();
        }

        var soloList = enabledList.Where(info => info.solo).ToList();
        if (soloList.Count > 0)
        {
            enabledList = soloList;
        }

        if (enabledList.Count == 0)
        {
            Singleton.Instance.runManager.GoToMap();
            yield break;
        }

        int rand = Random.Range(0, enabledList.Count);
        var chosenInfo = enabledList[rand];

        Task dialogueTask = new Task(chosenInfo.dialogue.PlayDialogue(dc));
        while (dialogueTask.Running)
            yield return null;

        Singleton.Instance.runManager.GoToMap();
    }
}
