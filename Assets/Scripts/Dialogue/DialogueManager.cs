using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public DialogueBox dialogueBox;
    [HideInInspector] public Dialogue nextSpecificDialogue; // Set this before changing scene to Event_Specific and this dialogue will play

    // Internal queue entry type, so we can pass both Dialogue and pause flag
    private class DialogueEntry
    {
        public Dialogue dialogue;
        public bool pause;

        public DialogueEntry(Dialogue d, bool p)
        {
            dialogue = d;
            pause = p;
        }
    }

    // Queue of pending dialogues
    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();

    // Flag to indicate whether we're currently processing the queue
    private bool isProcessingQueue = false;

    private void Awake()
    {
        dialogueBox.HideDialogueBox();
    }

    /// <summary>
    /// Immediately starts playing a single Dialogue (does NOT enqueue).
    /// </summary>
    public void PlayDialogue(Dialogue d, bool pause = false)
    {
        StartCoroutine(DialogueTaskRoutine(d, pause));
    }

    /// <summary>
    /// Queues a Dialogue to play after any currently playing or already-queued dialogues. 
    /// If nothing is playing, this will start processing the queue immediately.
    /// </summary>
    public void QueueDialogue(Dialogue d, bool pause = false)
    {
        if (d == null)
        {
            Debug.LogWarning("Tried to queue dialogue but dialogue was null");
            return;
        }

        dialogueQueue.Enqueue(new DialogueEntry(d, pause));

        // If we're not already running through the queue, start now
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessDialogueQueue());
        }
    }

    /// <summary>
    /// Coroutine that processes all enqueued Dialogues one after the other.
    /// </summary>
    private IEnumerator ProcessDialogueQueue()
    {
        isProcessingQueue = true;

        while (dialogueQueue.Count > 0)
        {
            DialogueEntry entry = dialogueQueue.Dequeue();
            // Wait for this dialogue to finish before continuing to the next
            yield return StartCoroutine(DialogueTaskRoutine(entry.dialogue, entry.pause));
        }

        isProcessingQueue = false;
    }

    /// <summary>
    /// Plays a single Dialogue and waits until it's done. Handles pause/unpause and hiding the box.
    /// </summary>
    public IEnumerator DialogueTaskRoutine(Dialogue d, bool pause = false)
    {
        if (d == null)
        {
            Debug.LogWarning("Tried to play dialogue but dialogue was null");
            yield break;
        }

        if (pause)
        {
            Singleton.Instance.pauseManager.SetPaused(true);
        }

        dialogueBox.gameObject.SetActive(true);
        Singleton.Instance.pauseManager.SetPaused(true);
        DialogueContext dc = new DialogueContext();
        dc.dialogueBox = dialogueBox;

        Task t = new Task(d.PlayDialogue(dc));
        while (t.Running)
        {
            yield return null;
        }

        Singleton.Instance.pauseManager.SetPaused(false);
        dialogueBox.HideDialogueBox();
        yield return new WaitForSeconds(0.5f);
        dialogueBox.gameObject.SetActive(false);

        if (pause)
        {
            Singleton.Instance.pauseManager.SetPaused(false);
        }
    }

    /// <summary>
    /// (Existing overload) Plays a list of DialogueTask sequences in order.
    /// </summary>
    public IEnumerator DialogueTaskRoutine(List<DialogueTask> dialogueTasks)
    {
        if (dialogueTasks == null)
        {
            Debug.LogWarning("Tried to play dialogue tasks but list was null");
            yield break;
        }

        dialogueBox.gameObject.SetActive(true);
        Singleton.Instance.pauseManager.SetPaused(true);
        DialogueContext dc = new DialogueContext();
        dc.dialogueBox = dialogueBox;
        dc.dialogueBox.HideAllChoiceButtons();

        foreach (var dt in dialogueTasks)
        {
            Task t = new Task(dt.RunTask(dc));
            while (t.Running)
            {
                yield return null;
            }
        }

        Singleton.Instance.pauseManager.SetPaused(false);
        dialogueBox.HideDialogueBox();
        yield return new WaitForSeconds(0.5f);
        dialogueBox.gameObject.SetActive(false);
    }

    public void SetNextSpecificDialogue(Dialogue d)
    {
        nextSpecificDialogue = d;
    }
}
