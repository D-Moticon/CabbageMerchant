using System.Collections;
using UnityEngine;
using Febucci.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class GameHintManager : MonoBehaviour
{
    [Header("Typewriter & UI")]
    public TypewriterByCharacter hintTypewriter;
    public CanvasGroup bubbleCanvasGroup;

    [Header("Hint Content")]
    public int noWeaponUsedRoundsBeforeHint = 2;
    [TextArea]public string noWeaponUsedLatelyHint;
    [TextArea]public string noWeaponUsedEverHint;
    [TextArea] public string noCabbagesBonkedHint;
    [TextArea] public string needToDragItemsHint;
    [TextArea] public string cantAffordItemHint;
    [TextArea] public string keyHint;
    [TextArea] public string itemMergeHint;
    [FormerlySerializedAs("mergeCabbageHint")] [TextArea] public string cabbageMergeHint;
    private bool cabbageMergeHintGiven = false;
    public int maxNeedToDragItemsFires = 5;
    private int needToDragItemsCounter = 0;

    [Header("Fade Settings")]
    [Tooltip("Seconds for bubble fade in/out")]
    public float bubbleFadeDuration = 0.25f;
    public float hintLingerTime = 10f;
    
    private int weaponRoundCounter = 0;
    private int weaponUsedCount = 0;
    private int cabbagesHitThisBounce=0;

    private Coroutine hintCoroutine;
    private string queuedHint;

    private bool keyHintGiven = false;
    private bool playerHasMergedItemsBefore = false;
    
    private void OnEnable()
    {
        Item.WeaponTriggeredEvent += WeaponTriggeredListener;
        GameStateMachine.ExitingBounceStateAction += ExitingBounceStateListener;
        GameStateMachine.BallFiredEvent += BallFiredListener;
        GameStateMachine.ExitingScoringAction += ExitingScoringListener;
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
        ItemManager.ItemClickedEvent += ItemClickedListener;
        bubbleCanvasGroup.alpha = 0f;
        Key.KeyCollectedEvent += KeyCollectedListener;
        RunManager.SceneChangedEvent += SceneChangedListener;
        ItemManager.ItemsMergedEvent += ItemsMergedListener;
        ItemManager.ItemPurchasedEvent += ItemPurchasedListener;
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
    }

    private void OnDisable()
    {
        Item.WeaponTriggeredEvent -= WeaponTriggeredListener;
        GameStateMachine.ExitingBounceStateAction -= ExitingBounceStateListener;
        GameStateMachine.BallFiredEvent -= BallFiredListener;
        GameStateMachine.ExitingScoringAction -= ExitingScoringListener;
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
        ItemManager.ItemClickedEvent -= ItemClickedListener;
        Key.KeyCollectedEvent -= KeyCollectedListener;
        RunManager.SceneChangedEvent -= SceneChangedListener;
        ItemManager.ItemsMergedEvent -= ItemsMergedListener;
        ItemManager.ItemPurchasedEvent -= ItemPurchasedListener;
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
    }

    private void WeaponTriggeredListener(Item item)
    {
        weaponRoundCounter = -1;
        weaponUsedCount++;
    }

    private void ExitingBounceStateListener()
    {
        weaponRoundCounter++;
        CheckCabbagesBonkedHint();
        CheckWeaponHint();
        CheckQueuedHint();
    }

    void CheckCabbagesBonkedHint()
    {
        if (cabbagesHitThisBounce == 0)
        {
            GiveHint(noCabbagesBonkedHint);
        }
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
        if (!string.IsNullOrEmpty(queuedHint))
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

        cabbagesHitThisBounce = 0;
    }

    void ExitingScoringListener()
    {
        StopAllCoroutines();
        if (bubbleCanvasGroup.alpha > 0f)
        {
            StartCoroutine(FadeOutBubble());
        }
    }

    void CabbageBonkedListener(BonkParams bp)
    {
        cabbagesHitThisBounce++;
    }

    void ItemClickedListener(Item item)
    {
        if (Singleton.Instance.playerStats.coins < item.GetItemPrice())
        {
            GiveHint(cantAffordItemHint);
        }
        
        else if (needToDragItemsCounter < maxNeedToDragItemsFires)
        {
            StartCoroutine(ItemNeedToDragCoroutine());
        }
    }

    void FireUpListener()
    {
        
    }

    IEnumerator ItemNeedToDragCoroutine()
    {
        float countdown = 0.25f;
        while (countdown > 0f)
        {
            if (Singleton.Instance.playerInputManager.fireUp)
            {
                GiveHint(needToDragItemsHint);
                needToDragItemsCounter++;
                break;
            }
            countdown -= Time.deltaTime;
            yield return null;
        }
    }

    void KeyCollectedListener()
    {
        if (keyHintGiven)
        {
            return;
        }
        
        GiveHint(keyHint);
        keyHintGiven = true;
    }

    void SceneChangedListener(string s)
    {
        StartCoroutine(FadeOutBubble());
    }

    void ItemsMergedListener(Item itemA, Item itemB)
    {
        playerHasMergedItemsBefore = true;
    }

    void ItemPurchasedListener(Item item)
    {
        if (playerHasMergedItemsBefore)
            return;

        var ownedItems = Singleton.Instance.itemManager.GetItemsInInventory();

        // Group by itemName, then only consider groups of size >1
        // where the base item has an upgradedItem to merge into:
        bool hasMergeableDupes = ownedItems
            .GroupBy(i => i.itemName)
            .Any(g => g.Count() > 1 
                      && g.First().upgradedItem != null);

        if (hasMergeableDupes)
        {
            GiveHint(itemMergeHint);
        }
    }


    void CabbageMergedListener(Cabbage.CabbageMergedParams cmp)
    {
        if (cabbageMergeHintGiven)
        {
            return;
        }

        cabbageMergeHintGiven = true;
        
        QueueHintUntilBouncingDone(cabbageMergeHint);
    }
}
