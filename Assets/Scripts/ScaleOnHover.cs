using UnityEngine;

/// <summary>
/// Smoothly scales the GameObject up when the mouse is within a specified radius of its position,
/// and back to original scale when the mouse leaves the radius.
/// Uses a centralized world-space mouse position provider and draws the hover radius in the editor.
/// </summary>
public class ScaleOnHover : MonoBehaviour
{
    [Tooltip("Scale multiplier when hovered.")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    [Tooltip("Speed of the scaling transition.")]
    [SerializeField] private float lerpSpeed = 5f;
    [Tooltip("Radius around the object's position to detect mouse hover.")]
    [SerializeField] private float hoverRadius = 1f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered;

    public SFXInfo hoverSFX;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // Smoothly interpolate towards the target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, lerpSpeed * Time.deltaTime);

        // Determine hover status based on distance to mouse
        bool nowHovered = CheckWithinRadius();
        if (nowHovered && !isHovered)
        {
            isHovered = true;
            targetScale = Vector3.Scale(originalScale, hoverScale);
            hoverSFX.Play();
        }
        else if (!nowHovered && isHovered)
        {
            isHovered = false;
            targetScale = originalScale;
        }
    }

    private bool CheckWithinRadius()
    {
        Vector3 worldMousePos = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        float sqrDist = (worldMousePos - transform.position).sqrMagnitude;
        return sqrDist <= hoverRadius * hoverRadius;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw hover radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hoverRadius);
    }
}
