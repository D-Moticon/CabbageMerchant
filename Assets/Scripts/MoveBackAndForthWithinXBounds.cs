using UnityEngine;

public class MoveBackAndForthWithinXBounds : MonoBehaviour
{
    [Tooltip("Movement speed (units per second).")]
    public float speed = 2f;

    public float expand = .5f;

    private float leftBound;
    private float rightBound;

    // Direction: +1 means moving right, -1 means moving left
    private int direction = 1;

    private void Start()
    {
        // We'll assume BoardMetrics.gridBounds.x is the total width,
        // so half of that is from center to edge.
        float halfWidth = Singleton.Instance.boundsManager.gridBounds.x * 0.5f;

        // Position is relative to the object's current local position
        // so let's define the leftBound and rightBound from that.
        float x = 0f;
        leftBound  = x - halfWidth - expand;
        rightBound = x + halfWidth + expand;
    }

    private void Update()
    {
        // Move horizontally at 'speed', in the current direction.
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        // Check if we've passed beyond the left or right bounds
        if (transform.position.x <= leftBound)
        {
            direction = 1; // switch to moving right
        }
        else if (transform.position.x >= rightBound)
        {
            direction = -1; // switch to moving left
        }
    }
}