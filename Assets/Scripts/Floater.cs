using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class Floater : MonoBehaviour
{
    [SerializeField] private TMP_Text floaterText;
    [SerializeField] private MMF_Player feedbackPlayer;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Material normalMaterial;
    public Material holofoilMaterial;

    private FloaterManager floaterManager;

    public void Initialize(FloaterManager manager)
    {
        floaterManager = manager;
    }

    public void Activate(string text, Color? textColor = null, Sprite sprite = null, bool isHolofoil = false)
    {
        floaterText.text = text;

        if (textColor.HasValue)
            floaterText.color = textColor.Value;

        if (sprite != null && spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = sprite;

            if (isHolofoil)
            {
                spriteRenderer.material = holofoilMaterial;
            }

            else
            {
                spriteRenderer.material = normalMaterial;
            }
        }

        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
        
        feedbackPlayer.PlayFeedbacks();
    }

    // Call this method at the end of your MMF Player's feedback (via events)
    public void ReturnToPool()
    {
        //floaterManager.ReturnFloater(this);
    }
}