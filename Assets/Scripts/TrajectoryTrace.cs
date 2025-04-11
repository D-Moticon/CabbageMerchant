using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryTrace : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Launcher launcher;

    [Header("Trajectory Settings")]
    public int maxBounces = 3;
    public float maxPredictionTime = 3f;
    public LayerMask collisionLayers;
    public int subStepsPerFixedUpdate = 5; // High precision substeps

    private LineRenderer lineRenderer;
    private Rigidbody2D ballRb;
    private float ballRadius;
    private float bounciness;
    private float fixedDeltaTime;
    private Vector2 colliderOffset;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        ballRb = launcher.ballPooledObject.prefab.GetComponent<Rigidbody2D>();
        var collider = launcher.ballPooledObject.prefab.GetComponent<CircleCollider2D>();

        ballRadius = collider ? collider.radius * launcher.ballPooledObject.prefab.transform.localScale.x : 0.1f;
        colliderOffset = collider ? collider.offset : Vector2.zero;
        bounciness = collider && collider.sharedMaterial ? collider.sharedMaterial.bounciness : 0f;

        fixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        DrawUltraPreciseTrajectory();
    }

    private void DrawUltraPreciseTrajectory()
    {
        Vector2 position = (Vector2)launcher.transform.position + colliderOffset;
        Vector2 velocity = (target.position - launcher.transform.position).normalized * launcher.launchSpeed;
        float gravity = Physics2D.gravity.y * ballRb.gravityScale;

        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, position - colliderOffset);

        float elapsedTime = 0f;
        int bounceCount = 0;
        float subStep = fixedDeltaTime / subStepsPerFixedUpdate;

        while (elapsedTime < maxPredictionTime && bounceCount <= maxBounces)
        {
            for (int i = 0; i < subStepsPerFixedUpdate; i++)
            {
                velocity += Vector2.up * gravity * subStep;
                velocity *= Mathf.Clamp01(1f - ballRb.linearDamping * subStep); // account for drag

                Vector2 displacement = velocity * subStep;
                float distance = displacement.magnitude;

                RaycastHit2D hit = Physics2D.CircleCast(position, ballRadius, displacement.normalized, distance, collisionLayers);

                if (hit.collider != null)
                {
                    position = hit.point + hit.normal * ballRadius;
                    velocity = Vector2.Reflect(velocity, hit.normal) * bounciness;

                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, position - colliderOffset);

                    bounceCount++;
                    if (velocity.magnitude < 0.05f) return;

                    break; // Break out to next main step after collision
                }
                else
                {
                    position += displacement;
                }

                elapsedTime += subStep;
            }

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position - colliderOffset);
        }
    }
}
