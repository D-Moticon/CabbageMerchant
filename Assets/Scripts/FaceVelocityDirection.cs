using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FaceVelocityDirection : MonoBehaviour
{
    [Tooltip("Additional rotation offset in degrees.")]
    public float rotationOffset = 0f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude > 0.0001f) // Ensure we only rotate if moving
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;            
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }
}