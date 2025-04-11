using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ToolTip : MonoBehaviour
{
    [Header("Tooltip UI References")]
    public CanvasGroup tooltipCanvasGroup;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text rarityText;
    public Image itemImage;

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

    private void Start()
    {
        // Start fully invisible
        tooltipCanvasGroup.alpha = 0f;
        tooltipCanvasGroup.interactable = false;
        tooltipCanvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
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
                    ShowTooltip(hoverable);
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
    private void ShowTooltip(IHoverable hoverable)
    {
        if (hoverable == null) return;

        titleText.text       = hoverable.GetTitleText();
        descriptionText.text = hoverable.GetDescriptionText();
        rarityText.text      = hoverable.GetRarityText();
        itemImage.sprite     = hoverable.GetImage();

        FadeCanvasGroup(1f);
    }

    /// <summary>
    /// Fades the tooltip out to invisible.
    /// </summary>
    private void HideTooltip()
    {
        FadeCanvasGroup(0f);
    }

    /// <summary>
    /// Positions the tooltip in the world, in front of the camera.
    /// </summary>
    /// <param name=\"mouseScreenPos\">Mouse position in screen coordinates</param>
    private void PositionTooltip(Vector2 mouseWorldPos)
    {
        
        transform.position = mouseWorldPos+worldOffset;
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
