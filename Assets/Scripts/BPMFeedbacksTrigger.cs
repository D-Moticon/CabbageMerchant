using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

public class BPMFeedbacksTrigger : MonoBehaviour, IBPM
{
    [Header("BPM Settings")]
    [Tooltip("Beats per minute")]
    public float bpm = 120f;
    
    [Tooltip("Apply trigger every X beats (set to 1 for every beat)")]
    public int modulationBeatInterval = 1;
    
    [Header("Trigger Delay")]
    [Tooltip("Delay the trigger by a number of beats (for example, 1 means delay by one beat)")]
    public float triggerDelayInBPM = 0f;
    
    [Header("Feedbacks to Trigger")]
    [Tooltip("Array of MMF_Player components to trigger.")]
    public MMF_Player[] feedbackPlayers;
    
    // Beat period in seconds.
    private float beatPeriod;
    // Timer tracking the time within the current beat.
    private float timeSinceLastBeat;
    // Counter tracking the number of beats passed.
    private int currentBeatCount;
    
    void Start()
    {
        beatPeriod = 60f / bpm;
        timeSinceLastBeat = 0f;
        currentBeatCount = 0;
        
        // If no feedback players are assigned, try to find them in children.
        if (feedbackPlayers == null || feedbackPlayers.Length == 0)
        {
            Debug.LogWarning("No MMF_Player assigned. Attempting to find MMF_Player components in children.");
            feedbackPlayers = GetComponentsInChildren<MMF_Player>();
            if (feedbackPlayers.Length == 0)
            {
                Debug.LogError("No MMF_Player components found on or under " + gameObject.name);
                enabled = false;
                return;
            }
        }
    }
    
    void Update()
    {
        timeSinceLastBeat += Time.deltaTime;
        
        // Process beats when the timer exceeds the beat period.
        while (timeSinceLastBeat >= beatPeriod)
        {
            timeSinceLastBeat -= beatPeriod;
            currentBeatCount++;
            
            // Check if this beat should trigger the feedback.
            if (modulationBeatInterval <= 0 || (currentBeatCount % modulationBeatInterval == 0))
            {
                // Compute the delay in seconds based on the provided BPM delay.
                float delaySeconds = triggerDelayInBPM * beatPeriod;
                StartCoroutine(TriggerFeedbacksAfterDelay(delaySeconds));
            }
        }
    }
    
    private IEnumerator TriggerFeedbacksAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (MMF_Player player in feedbackPlayers)
        {
            if (player != null)
            {
                player.PlayFeedbacks();
            }
        }
    }

    public void OffsetBeat(float offset)
    {
        triggerDelayInBPM = offset;
    }
}
