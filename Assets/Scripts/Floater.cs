using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class Floater : MonoBehaviour
{
    [SerializeField] private TMP_Text floaterText;
    [SerializeField] private MMF_Player feedbackPlayer;

    private FloaterManager floaterManager;

    public void Initialize(FloaterManager manager)
    {
        floaterManager = manager;
    }

    public void Activate(string text, Color? textColor = null)
    {
        floaterText.text = text;

        if (textColor.HasValue)
            floaterText.color = textColor.Value;

        feedbackPlayer.PlayFeedbacks();
    }

    // Call this method at the end of your MMF Player's feedback (via events)
    public void ReturnToPool()
    {
        //floaterManager.ReturnFloater(this);
    }
}