/* ─────────────────── BallRailController.cs  (ray-snap in Update) ─────────────────── */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BallRailController : MonoBehaviour
{
    [Header("Speed clamp (units/s)")]
    public float minSpeed = 8f;
    public float maxSpeed = 30f;

    [Header("Raycast margin (m)")]
    public float skin = 0.02f;

    [Header("Debug visuals")]
    public bool showDebugLines = true;

    Rigidbody2D      rb;
    CircleCollider2D col;

    bool   grinding;
    Vector2 lockedDir;           // ±X sign captured on first hit
    float   speed;
    float   savedGravity;

    int ballLayer, railLayer, railMask;

    void Awake()
    {
        rb        = GetComponent<Rigidbody2D>();
        col       = GetComponent<CircleCollider2D>();

        ballLayer = gameObject.layer;
        railLayer = LayerMask.NameToLayer("Rail");
        railMask  = 1 << railLayer;
    }

    /* ───────── first collision starts grind ───────── */
    void OnCollisionEnter2D(Collision2D c)
    {
        if (grinding || c.collider.gameObject.layer != railLayer) return;

        Vector2 pt     = c.GetContact(0).point;
        Vector2 normal = c.GetContact(0).normal;

        lockedDir = rb.linearVelocity.x >= 0 ? Vector2.right : Vector2.left;
        speed     = Mathf.Clamp(rb.linearVelocity.magnitude, minSpeed, maxSpeed);

        savedGravity      = rb.gravityScale;
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;

        Physics2D.IgnoreLayerCollision(ballLayer, railLayer, true);

        rb.position       = pt + normal * col.radius;
        rb.linearVelocity = lockedDir * speed;

        grinding = true;
        Debug.Break();   // pause once for inspection
    }

    /* ───────── per-frame grind logic ───────── */
    void Update()
    {
        if (!grinding) return;

        Vector2 v = rb.linearVelocity.sqrMagnitude < 0.01f
                    ? lockedDir * minSpeed * Time.deltaTime
                    : rb.linearVelocity;

        Vector2 perp = lockedDir == Vector2.right
                       ? new Vector2(-v.y, v.x)
                       : new Vector2( v.y, -v.x);
        perp.Normalize();

        Vector2 origin = rb.position;
        float   rayLen = col.radius + skin;

        if (showDebugLines)
            Debug.DrawLine(origin, origin + perp * rayLen, Color.yellow);

        RaycastHit2D hit = Physics2D.Raycast(origin, perp, rayLen, railMask);

        if (!hit)
        {
            EndGrind();
            return;
        }

        Vector2 normal  = hit.normal;
        if (showDebugLines)
            Debug.DrawLine(hit.point, hit.point + normal, Color.red);

        Vector2 tangent = lockedDir == Vector2.right
                          ? new Vector2(-normal.y,  normal.x)
                          : new Vector2( normal.y, -normal.x);
        tangent.Normalize();

        if (showDebugLines)
            Debug.DrawLine(hit.point, hit.point + tangent * 0.5f, Color.cyan);

        rb.position       = hit.point + normal * col.radius;
        rb.linearVelocity = tangent * speed;
    }

    /* ───────── restore physics & collisions ───────── */
    void EndGrind()
    {
        if (!grinding) return;

        grinding          = false;
        rb.gravityScale   = savedGravity;
        rb.freezeRotation = false;
        Physics2D.IgnoreLayerCollision(ballLayer, railLayer, false);
    }
}
