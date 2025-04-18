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
    
    [Tooltip("Only enabled DialogueInfos will be considered for playback.")]
    public List<DialogueInfo> dialoguesToChooseFrom;
    
    void Start()
    {
        dialogueBox.HideDialogueBox();
        DialogueContext dc = new DialogueContext { dialogueBox = dialogueBox };
        // kick off our coroutine via your Task system
        Task t = new Task(PlayDialogueTask(dc));
    }

    private IEnumerator PlayDialogueTask(DialogueContext dc)
    {
        // filter only enabled dialogues
        var enabledList = dialoguesToChooseFrom
            .Where(info => info.isEnabled)
            .ToList();

        if (enabledList.Count == 0)
        {
            // no enabled dialogues, proceed to map
            Singleton.Instance.runManager.GoToMap();
            yield break;
        }

        // pick a random enabled dialogue
        int rand = Random.Range(0, enabledList.Count);
        var chosenInfo = enabledList[rand];

        // optionally disable it so it won't play again
        // chosenInfo.isEnabled = false;

        // play it
        Task dialogueTask = new Task(chosenInfo.dialogue.PlayDialogue(dc));
        while (dialogueTask.Running)
            yield return null;

        // after dialogue, go to map
        Singleton.Instance.runManager.GoToMap();
    }
}