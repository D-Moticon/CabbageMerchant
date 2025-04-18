using System.Collections;
using UnityEngine;
using Febucci.UI;

public class GameHintManager : MonoBehaviour
{
    [Header("Typewriter & UI")]
    public TypewriterByCharacter hintTypewriter;
    public CanvasGroup bubbleCanvasGroup;

    [Header("Hint Content")]
    public int noWeaponUsedRoundsBeforeHint = 2;
    [TextArea]public string noWeaponUsedLatelyHint;
    [TextArea]public string noWeaponUsedEverHint;

    [Header("Fade Settings")]
    [Tooltip("Seconds for bubble fade in/out")]
    public float bubbleFadeDuration = 0.25f;
    public float hintLingerTime = 10f;
    
    private int weaponRoundCounter = 0;
    private int weaponUsedCount = 0;

    private Coroutine hintCoroutine;
    private string queuedHint;

    private void OnEnable()
    {
        Item.WeaponTriggeredEvent += WeaponTriggeredListener;
        GameStateMachine.ExitingBounceStateAction += ExitingBounceStateListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
        GameStateMachine.ExitingScoringAction += ExitingScoringListener;
        bubbleCanvasGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        Item.WeaponTriggeredEvent -= WeaponTriggeredListener;
        GameStateMachine.ExitingBounceStateAction -= ExitingBounceStateListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
        GameStateMachine.ExitingScoringAction -= ExitingScoringListener;
    }

    private void WeaponTriggeredListener(Item item)
    {
        weaponRoundCounter = -1;
        weaponUsedCount++;
    }

    private void ExitingBounceStateListener()
    {
        weaponRoundCounter++;
        CheckWeaponHint();
        CheckQueuedHint();
    }

    private void CheckWeaponHint()
    {
        if (Singleton.Instance.itemManager.GetWeaponCount() == 0)
        {
            return;
        }
        if (weaponRoundCounter >= noWeaponUsedRoundsBeforeHint)
        {
            // If player never used a weapon at all
            if (weaponUsedCount == 0)
            {
                GiveHint(noWeaponUsedEverHint);
            }
            else
            {
                // Player used a weapon before but hasn't in a while
                GiveHint(noWeaponUsedLatelyHint);
            }
            weaponRoundCounter = 0;
        }
    }

    void CheckQueuedHint()
    {
        if (queuedHint != null)
        {
            GiveHint(queuedHint);
            queuedHint = null;
        }
    }

    public void GiveHint(string hint)
    {
        // Ensure only one hint coroutine runs at a time
        if (hintCoroutine != null)
            StopCoroutine(hintCoroutine);
        hintCoroutine = StartCoroutine(HintCoroutine(hint));
    }

    public void QueueHintUntilBouncingDone(string hint)
    {
        print(hint);
        queuedHint = hint;
    }

    private IEnumerator HintCoroutine(string hint)
    {
        // Fade in the bubble
        yield return StartCoroutine(FadeInBubble());

        // Show the hint text
        hintTypewriter.ShowText(hint);
        while (hintTypewriter.isShowingText)
            yield return null;

        yield return new WaitForSeconds(hintLingerTime);
        
        // Fade out the bubble
        yield return StartCoroutine(FadeOutBubble());

        hintCoroutine = null;
    }

    private IEnumerator FadeInBubble()
    {
        float elapsed = 0f;
        float startAlpha = bubbleCanvasGroup.alpha;
        float endAlpha = 1f;
        bubbleCanvasGroup.gameObject.SetActive(true);

        while (elapsed < bubbleFadeDuration)
        {
            elapsed += Time.deltaTime;
            bubbleCanvasGroup.alpha = Mathf.Lerp(startAlpha,endAlpha, (elapsed / bubbleFadeDuration));
            yield return null;
        }
        bubbleCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutBubble()
    {
        float elapsed = 0f;

        float startAlpha = bubbleCanvasGroup.alpha;
        float endAlpha = 0f;
        
        while (elapsed < bubbleFadeDuration)
        {
            elapsed += Time.deltaTime;
            bubbleCanvasGroup.alpha = Mathf.Lerp(startAlpha,endAlpha, (elapsed / bubbleFadeDuration));
            yield return null;
        }

        bubbleCanvasGroup.alpha = 0f;
        bubbleCanvasGroup.gameObject.SetActive(false);
    }

    void BallFiredListener(Ball ball)
    {
        StopAllCoroutines();
        if (bubbleCanvasGroup.alpha > 0f)
        {
            StartCoroutine(FadeOutBubble());
        }
    }

    void ExitingScoringListener()
    {
        StopAllCoroutines();
        if (bubbleCanvasGroup.alpha > 0f)
        {
            StartCoroutine(FadeOutBubble());
        }
    }
}
