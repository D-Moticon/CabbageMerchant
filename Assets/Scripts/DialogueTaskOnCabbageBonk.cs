using System;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using MoreMountains.Feedbacks;

public class DialogueTaskOnCabbageBonk : MonoBehaviour
{
    public Cabbage owningCabbage;
    public int bonksToPop = 5;
    private int bonkCountdown = 5;
    public TMP_Text bonkCountdownText;
    public TMP_Text normalCabbageScoreText;
    public Dialogue dialogue;
    public GameObject characterParent;
    public GameObject bubbleParent;
    public SpriteRenderer characterSpriteRenderer;
    public MMF_Player bonkCountdownFeel;
    public PooledObjectData popVFX;
    public SFXInfo popSFX;
    private bool hasPopped = false;
    
    private void OnEnable()
    {
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
        Cabbage.CabbageMergedEventPreDestroy += CabbageMergedListener;
        characterParent.SetActive(false);
        bubbleParent.SetActive(false);
        bonkCountdown = bonksToPop;
        bonkCountdownText.text = bonkCountdown.ToString();
        hasPopped = false;
        bonkCountdownText.enabled = true;
        normalCabbageScoreText.enabled = false;
    }

    private void OnDisable()
    {
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
        Cabbage.CabbageMergedEventPreDestroy -= CabbageMergedListener;
    }

    public void SetBonkCountdown(int newBonkCountdown)
    {
        characterParent.SetActive(true);
        bubbleParent.SetActive(true);
        bonkCountdown = newBonkCountdown;
        bonkCountdownText.text = bonkCountdown.ToString();
        hasPopped = false;
        bonkCountdownText.enabled = true;
        normalCabbageScoreText.enabled = false;
    }
    
    private void CabbageMergedListener(Cabbage.CabbageMergedParams cpp)
    {
        if (hasPopped)
        {
            return;
        }
        
        if (cpp.oldCabbageA != owningCabbage && cpp.oldCabbageB != owningCabbage)
        {
            return;
        }
        
        StartDialogue();
    }
    
    private void CabbageBonkedListener(BonkParams bp)
    {
        if (hasPopped)
        {
            return;
        }
        
        if (bp.bonkedCabbage != owningCabbage)
        {
            return;
        }

        bonkCountdown -= 1;

        bonkCountdownText.text = bonkCountdown.ToString();
        bonkCountdownFeel.PlayFeedbacks();
        
        if (bonkCountdown <= 0)
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        Singleton.Instance.dialogueManager.QueueDialogue(dialogue, true);
        characterParent.SetActive(false);
        bubbleParent.SetActive(false);
        if (popVFX != null) popVFX.Spawn(this.transform.position);
        popSFX.Play(this.transform.position);
        hasPopped = true;
        normalCabbageScoreText.enabled = true;
        bonkCountdownText.enabled = false;
    }

    public void SetDialogue(Dialogue d)
    {
        dialogue = d;
    }

    public void SetIcon(Sprite s)
    {
        characterParent.SetActive(true);
        bubbleParent.SetActive(true);
        characterSpriteRenderer.sprite = s;
    }
}
