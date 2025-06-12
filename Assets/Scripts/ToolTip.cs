using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Febucci.UI;

public class ToolTip : MonoBehaviour
{
    [Header("Tooltip UI References")]
    public CanvasGroup tooltipCanvasGroup;
    public TextAnimator_TMP titleTextAnimator;
    public TMP_Text typeText;
    public TextAnimator_TMP descriptionTextAnimator;
    public TMP_Text rarityText;
    public TMP_Text triggerText;
    public TMP_Text valueText;
    public Transform valueHeader;
    public Image itemImage;
    public Material normalMat;
    public Material holofoilMat;

    [Header("World-Space Settings")]
    [Tooltip("How far in front of the Camera should this tooltip appear?")]
    public float distanceFromCamera = 10f;
    [Tooltip("Offset in world space relative to the computed tooltip position.")]
    public Vector2 worldOffset = new Vector2(0.3f, 0.3f);

    [Header("Fade Settings")]
    [Tooltip("Time (in seconds) to fade the tooltip in or out.")]
    public float fadeDuration = 0.2f;

    private IHoverable currentHover;
    private IHoverable overrideHoverable;
    private Coroutine fadeRoutine;
    private bool IsUsingOverride => overrideHoverable != null;

    public delegate void HoverableDelegate(IHoverable hoverable);
    public static event HoverableDelegate HoverableHoveredEvent;

    private void Start()
    {
        // start hidden & non-interactable
        tooltipCanvasGroup.alpha = 0f;
        tooltipCanvasGroup.blocksRaycasts = false;
        tooltipCanvasGroup.interactable   = false;
    }

    private void Update()
    {
        // override mode bypasses normal hover
        if (IsUsingOverride)
        {
            PositionTooltip(Singleton.Instance.playerInputManager.mousePosWorldSpace);
            return;
        }
        CheckForHover(false);
    }

    public void ForceToolTipUpdate() => CheckForHover(true);

    void CheckForHover(bool updateIfSame = false)
    {
        Vector2 mouseWorldPos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        PositionTooltip(mouseWorldPos);

        var openPanels = Singleton.Instance.menuManager.allPanels
            .Where(p => p.gameObject.activeInHierarchy)
            .Select(p => p.transform)
            .ToArray();
        bool panelOpen = openPanels.Length > 0;

        var hits = Physics2D.OverlapPointAll(mouseWorldPos);
        foreach (var col in hits)
        {
            var hoverable = col.GetComponentInChildren<IHoverable>()
                           ?? col.GetComponentInParent<IHoverable>();
            if (hoverable == null) continue;

            if (panelOpen)
            {
                bool underPanel = openPanels.Any(panel => col.transform.IsChildOf(panel));
                if (!underPanel) continue;
            }

            if (hoverable != currentHover || updateIfSame)
            {
                currentHover = hoverable;
                var hm = new HoverableModifier();
                if (hoverable is Item item && item.isHolofoil)
                    hm.isHolofoil = true;

                ShowTooltip(hoverable, hm);
                HoverableHoveredEvent?.Invoke(hoverable);
            }
            return;
        }

        if (currentHover != null)
        {
            currentHover = null;
            HideTooltip();
        }
    }

    private void ShowTooltip(IHoverable hoverable, HoverableModifier hm = null)
    {
        if (hoverable == null) return;

        // cancel any running fade
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        overrideHoverable = null;

        // set fields
        SetTooltipFields(hoverable, hm ?? new HoverableModifier());

        // use FadeCanvasGroup for fade-in
        FadeCanvasGroup(1f);
    }

    public void HideTooltip()
    {
        overrideHoverable = null;
        FadeCanvasGroup(0f);
    }

    public void ShowOverrideTooltip(IHoverable customHoverable, HoverableModifier hm = null)
    {
        if (customHoverable == null)
        {
            HideTooltip();
            return;
        }

        overrideHoverable = customHoverable;
        SetTooltipFields(customHoverable, hm);
        FadeCanvasGroup(1f);
        PositionTooltip(Singleton.Instance.playerInputManager.mousePosWorldSpace);
    }

    private void SetTooltipFields(IHoverable hoverable, HoverableModifier hm)
    {
        titleTextAnimator.SetText(hoverable.GetTitleText(hm));
        descriptionTextAnimator.SetText(hoverable.GetDescriptionText(hm));
        typeText.text           = hoverable.GetTypeText(hm);
        rarityText.text         = hoverable.GetRarityText();
        triggerText.text        = hm?.takeTriggersFromItem?.GetTriggerText() ?? hoverable.GetTriggerText();
        valueText.text          = hoverable.GetValueText();
        valueHeader.gameObject.SetActive(!string.IsNullOrEmpty(valueText.text));
        itemImage.sprite        = hoverable.GetImage();
        itemImage.material      = (hm.isHolofoil ? holofoilMat : normalMat);
    }

    private void PositionTooltip(Vector2 mouseWorldPos)
    {
        // Compute the desired world position based on your worldOffset.
        Vector3 desiredWorldPos = (Vector3)mouseWorldPos + (Vector3)worldOffset;
    
        // Get the RectTransform of this tooltip.
        RectTransform rectTrans = GetComponentInChildren<RectTransform>();
        if (rectTrans == null)
        {
            // If no RectTransform is found, just assign the desired position
            transform.position = desiredWorldPos;
            return;
        }
    
        // Convert the desired world position to screen space.
        Vector3 screenPos = Camera.main.WorldToScreenPoint(desiredWorldPos);
    
        // Calculate half size of the tooltip in screen space.
        // Note: rectTrans.rect is in local space; its size can be scaled by rectTrans.lossyScale.
        Vector2 halfSize = new Vector2(
            rectTrans.rect.width * rectTrans.lossyScale.x * 0.5f*110f,
            rectTrans.rect.height * rectTrans.lossyScale.y * 0.5f *110f
        );

        // Clamp the screen position so that the tooltip's bounds don't go off-screen.
        screenPos.x = Mathf.Clamp(screenPos.x, halfSize.x, Screen.width - halfSize.x);
        screenPos.y = Mathf.Clamp(screenPos.y, halfSize.y, Screen.height - halfSize.y);
    
        // Convert the clamped screen position back to world space.
        Vector3 correctedWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
        
        // Preserve the original z coordinate (distance)
        correctedWorldPos.z = desiredWorldPos.z;
    
        transform.position = correctedWorldPos;
    }

    private void FadeCanvasGroup(float targetAlpha)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (targetAlpha > 0f)
        {
            tooltipCanvasGroup.blocksRaycasts = true;
            tooltipCanvasGroup.interactable   = true;
        }

        float start = tooltipCanvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            tooltipCanvasGroup.alpha = Mathf.Lerp(start, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        tooltipCanvasGroup.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            tooltipCanvasGroup.blocksRaycasts = false;
            tooltipCanvasGroup.interactable   = false;
        }
        fadeRoutine = null;
    }
}
