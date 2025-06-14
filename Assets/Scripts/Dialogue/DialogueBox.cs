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
    public TMP_Text dialogueText;
    public Image characterImage;
    public float targetImageHeight = 200f;
    public MMF_Player characterTalkPlayer;

    public DialogueButton[] choiceButtons;
    public Transform itemSlotParent;

    public CanvasGroup canvasGroup;
    public Canvas canvas;
    public bool isHidden = false;

    public int normalCanvasLayer = -5;
    public int topCanvasLayer = 100;
    
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
        if (sprite == null)
        {
            characterImage.sprite = null;
            return;
        }

        // assign & configure the Image
        characterImage.sprite = sprite;
        characterImage.type = Image.Type.Simple;
        characterImage.preserveAspect = true;

        // force the RectTransform height
        var rt = characterImage.rectTransform;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   targetImageHeight);

        // width will auto‑adjust because preserveAspect = true
        // BUT to be 100% safe (in case preserveAspect behaves differently),
        // we can calculate and set it manually:
        float nativeW = sprite.rect.width;
        float nativeH = sprite.rect.height;
        float aspect = nativeW / nativeH;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetImageHeight * aspect);
    }

    public void SetDialogueText(string text, bool animate = true)
    {
        if (animate)
        {
            dialogueTypewriter.ShowText(text);
        }

        else
        {
            dialogueTextAnimator.SetText(text);
        }
    }

    public void SetCharacterNameText(string text)
    {
        nameTextAnimator.SetText(text);
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
