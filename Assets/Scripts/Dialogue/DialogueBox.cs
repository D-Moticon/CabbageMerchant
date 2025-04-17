using UnityEngine;
using TMPro;
using Febucci.UI;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Feedbacks;

public class DialogueBox : MonoBehaviour
{
    public TextAnimator_TMP nameTextAnimator;

    public TextAnimator_TMP dialogueTextAnimator;
    public TypewriterByCharacter dialogueTypewriter;
    public Image characterImage;
    [Tooltip("If you ever show a square first, and you know your tall sprites are 1536px high, set this to 1536.")]
    public float fallbackVerticalHeightPx = 1536f;

    // runtime cache of the last vertical‐sprite height (in UI pixels)
    private float lastVerticalHeightPx = 0f;
    public MMF_Player characterTalkPlayer;

    public DialogueButton[] choiceButtons;
    public Transform itemSlotParent;

    public CanvasGroup canvasGroup;
    public bool isHidden = false;

    public void ActivateButtons(int quantity)
    {
        HideAllChoiceButtons();
        for (int i = 0; i < quantity; i++)
        {
            choiceButtons[i].gameObject.SetActive(true);
        }
    }

    public void HideAllChoiceButtons()
    {
        foreach (DialogueButton db in choiceButtons)
        {
            db.gameObject.SetActive(false);
            db.buttonPressed = false;
        }
    }

    public void SetCharacterImage(Sprite sprite)
    {
        if (sprite == null) return;

        // 1) Basic setup
        characterImage.sprite = sprite;
        characterImage.type = Image.Type.Simple;       // no 9‑slice / tiled overrides
        characterImage.preserveAspect = true;          // lock W/H ratio

        var rt = characterImage.rectTransform;
        float nativeW = sprite.rect.width;
        float nativeH = sprite.rect.height;

        if (nativeH > nativeW)
        {
            // — Tall sprite: show at native size and record its final height
            characterImage.SetNativeSize();
            lastVerticalHeightPx = rt.rect.height;
        }
        else
        {
            // — Square (or landscape): scale so height == recorded vertical height
            float targetH = (lastVerticalHeightPx > 0f) 
                ? lastVerticalHeightPx 
                : fallbackVerticalHeightPx;

            float scale = targetH / nativeH;
            float targetW = nativeW * scale;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   targetH*0.08f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetW*0.08f);
        }
    }

    public void HideDialogueBox()
    {
        canvasGroup.alpha = 0f;
        isHidden = true;
    }

    public IEnumerator FadeInDialogueBox(float duration)
    {
        StopAllCoroutines();

        isHidden = false;
        
        float elpasedTime = 0f;
        while (elpasedTime < duration)
        {
            float t = elpasedTime / duration;
            canvasGroup.alpha = t;
            elpasedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
    
    public IEnumerator FadeOutDialogueBox(float duration)
    {
        StopAllCoroutines();

        isHidden = true;
        
        float elpasedTime = 0f;
        while (elpasedTime < duration)
        {
            float t = elpasedTime / duration;
            canvasGroup.alpha = 1f-t;
            elpasedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
