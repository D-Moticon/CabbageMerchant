using UnityEngine;

public class Pendulum : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lineRenderer;
    public Transform head;

    [Header("Pendulum Settings")]
    public float length = 5f;
    public float period = 2f;
    [Tooltip("Maximum angle in degrees from the vertical")] 
    public float angle = 30f;

    private float angularFrequency;
    private float time;

    private void Start()
    {
        angularFrequency = 2 * Mathf.PI / period;
    }

    private void Update()
    {
        if (Singleton.Instance.pauseManager.isPaused) return;
        
        time += Time.deltaTime;

        // Calculate current swing angle using SHM
        float currentAngle = angle * Mathf.Cos(angularFrequency * time);

        // Determine head position along the pendulum arm
        Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * Vector3.down;
        Vector3 headPosition = transform.position + direction * length;

        // Update head position
        head.position = headPosition;

        // Rotate head to face along the pendulum arm
        // Assuming head's local 'up' points away from the pivot
        head.rotation = Quaternion.FromToRotation(Vector3.up, direction);

        // Update LineRenderer to draw the arm
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, headPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        // Draw limit positions
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, angle) * Vector3.down * length);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, -angle) * Vector3.down * length);

        Gizmos.color = Color.yellow;
        // Draw a small sphere at the default bottom
        Gizmos.DrawWireSphere(transform.position + Vector3.down * length, 0.1f);
    }
}