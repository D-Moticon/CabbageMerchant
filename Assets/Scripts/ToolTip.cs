using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
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
    public Image itemImage;
    public Material normalMat;
    public Material holofoilMat;

    [Header("World Space Settings")]
    [Tooltip("How far in front of the Camera should this tooltip appear?")]
    public float distanceFromCamera = 10f;

    [Tooltip("Offset in world space relative to the computed tooltip position.")]
    public Vector2 worldOffset = new Vector2(0.3f, 0.3f);

    [Header("Fade Settings")]
    [Tooltip("Time (in seconds) to fade the tooltip in or out.")]
    public float fadeDuration = 0.2f;

    private IHoverable currentHover;
    private Coroutine fadeRoutine;

    // If we're using a custom IHoverable override (e.g., for merging), store it here
    private IHoverable overrideHoverable = null;

    private bool IsUsingOverride => overrideHoverable != null;
    
    public delegate void HoverableDelegate(IHoverable hoverable);
    public static event HoverableDelegate HoverableHoveredEvent;
    
    private void Start()
    {
        // Start fully invisible
        tooltipCanvasGroup.alpha = 0f;
        tooltipCanvasGroup.interactable = false;
        tooltipCanvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        // If we have an overrideHoverable, we skip normal detection
        if (IsUsingOverride)
        {
            Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
            PositionTooltip(mousePos);
            return;
        }

        Vector2 mouseWorldPos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        PositionTooltip(mouseWorldPos);

        // Detect if we are hovering something that implements IHoverable
        Collider2D collider = Physics2D.OverlapPoint(mouseWorldPos);
        if (collider)
        {
            IHoverable hoverable = collider.GetComponentInChildren<IHoverable>()
                                    ?? collider.GetComponentInParent<IHoverable>();
            if (hoverable != null)
            {
                if (hoverable != currentHover)
                {
                    currentHover = hoverable;
                    
                    HoverableModifier hm = new HoverableModifier();
                    
                    Item item = hoverable as Item;
                    if (item != null)
                    {
                        if (item.isHolofoil)
                        {
                            hm.isHolofoil = true;
                        }
                    }
                    
                    ShowTooltip(hoverable, hm);
                    
                    HoverableHoveredEvent?.Invoke(hoverable);
                }
                return;
            }
        }

        // If no valid hoverable object is under the mouse
        if (currentHover != null)
        {
            currentHover = null;
            HideTooltip();
        }
    }

    /// <summary>
    /// Sets the tooltip fields from the hovered item's data, then fades it in.
    /// </summary>
    private void ShowTooltip(IHoverable hoverable, HoverableModifier hm = null)
    {
        if (hoverable == null) return;

        overrideHoverable = null;  // Clear any override
        SetTooltipFields(hoverable, hm);
        FadeCanvasGroup(1f);
    }

    /// <summary>
    /// Called to hide the tooltip (fades out).
    /// If we were overriding, that override is canceled.
    /// </summary>
    public void HideTooltip()
    {
        overrideHoverable = null;  
        FadeCanvasGroup(0f);
    }

    /// <summary>
    /// Replaces the normal hover logic with an override IHoverable (e.g. merging scenario).
    /// This sets overrideHoverable so normal detection is bypassed.
    /// Use HideTooltip() to revert to normal.
    /// </summary>
    public void ShowOverrideTooltip(IHoverable customHoverable, HoverableModifier hm = null)
    {
        if (customHoverable == null)
        {
            HideTooltip();
            return;
        }

        overrideHoverable = customHoverable;
        SetTooltipFields(overrideHoverable, hm);
        FadeCanvasGroup(1f);

        // Position near mouse
        Vector2 mousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        PositionTooltip(mousePos);
    }

    private void SetTooltipFields(IHoverable hoverable, HoverableModifier hm = null)
    {
        titleTextAnimator.SetText(hoverable.GetTitleText(hm));
        descriptionTextAnimator.SetText(hoverable.GetDescriptionText(hm));
        typeText.text = hoverable.GetTypeText(hm);
        rarityText.text      = hoverable.GetRarityText();
        itemImage.sprite     = hoverable.GetImage();
        triggerText.text     = hoverable.GetTriggerText();
        valueText.text = hoverable.GetValueText();

        if (hm != null)
        {
            if (hm.isHolofoil)
            {
                itemImage.material = holofoilMat;
            }

            else
            {
                itemImage.material = normalMat;
            }
        }

        else
        {
            itemImage.material = normalMat;
        }
    }

    /// <summary>
    /// Positions the tooltip near the mouse in 2D world space.
    /// </summary>
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

    /// <summary>
    /// Smoothly fades the tooltip's CanvasGroup to the target alpha.
    /// </summary>
    private void FadeCanvasGroup(float targetAlpha)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (targetAlpha > 0f)
        {
            tooltipCanvasGroup.blocksRaycasts = true;
            tooltipCanvasGroup.interactable   = true;
        }

        float startAlpha = tooltipCanvasGroup.alpha;
        float elapsed    = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            tooltipCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        tooltipCanvasGroup.alpha = targetAlpha;

        // Disable interaction if fully transparent
        if (targetAlpha <= 0f)
        {
            tooltipCanvasGroup.blocksRaycasts = false;
            tooltipCanvasGroup.interactable   = false;
        }

        fadeRoutine = null;
    }
}
