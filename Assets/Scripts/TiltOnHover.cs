using UnityEngine;

/// <summary>
/// Smoothly tilts the GameObject away from the mouse position within a specified hover box.
/// Rotates around local X based on vertical offset, and local Y based on horizontal offset.
/// Uses a centralized world-space mouse position provider and draws the hover zone in the editor.
/// </summary>
public class TiltOnHover : MonoBehaviour
{
    [Tooltip("Size of the hover detection box (X = width, Y = height) around the object's center.")]
    [SerializeField] private Vector2 hoverBoxSize = new Vector2(2f, 2f);
    [Tooltip("Maximum tilt angle in degrees for both axes.")]
    [SerializeField] private float maxTiltAngle = 15f;
    [Tooltip("Speed of the tilt interpolation.")]
    [SerializeField] private float lerpSpeed = 5f;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private bool isHovered;

    private void Awake()
    {
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }

    private void Update()
    {
        // Smoothly interpolate rotation toward target
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, lerpSpeed * Time.deltaTime);

        // Determine if mouse is within hover box
        bool nowHovered = CheckWithinBox();
        if (nowHovered && !isHovered)
        {
            isHovered = true;
        }
        else if (!nowHovered && isHovered)
        {
            isHovered = false;
            targetRotation = originalRotation;
        }

        if (isHovered)
        {
            ApplyTilt();
        }
    }

    private bool CheckWithinBox()
    {
        Vector3 worldMouse = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        // Convert world mouse to local space
        Vector3 localMouse = transform.InverseTransformPoint(worldMouse);

        // Check horizontal (X) and vertical (Y) extents
        float halfX = hoverBoxSize.x * 0.5f;
        float halfY = hoverBoxSize.y * 0.5f;
        return localMouse.x >= -halfX && localMouse.x <= halfX
            && localMouse.y >= -halfY && localMouse.y <= halfY;
    }

    private void ApplyTilt()
    {
        Vector3 worldMouse = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        Vector3 localMouse = transform.InverseTransformPoint(worldMouse);

        // Normalize offsets (-1 to 1)
        float halfX = hoverBoxSize.x * 0.5f;
        float halfY = hoverBoxSize.y * 0.5f;
        float normX = Mathf.Clamp(localMouse.x / halfX, -1f, 1f);
        float normY = Mathf.Clamp(localMouse.y / halfY, -1f, 1f);

        // Compute tilt angles: positive X offset tilts around Y, positive Y offset tilts around X
        float tiltX = -normY * maxTiltAngle;
        float tiltY = normX * maxTiltAngle;

        // Set target rotation in local space
        targetRotation = originalRotation * Quaternion.Euler(tiltX, tiltY, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw hover detection box
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(hoverBoxSize.x, hoverBoxSize.y, 0.1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}
