using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class OverworldCharacter : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second")]    public float speed = 5f;
    [Tooltip("How close to the target before we consider ourselves arrived")] public float stoppingDistance = 0.1f;

    [Header("Direction Sprites")]
    [Tooltip("Sprite when facing up")]    public Sprite upSprite;
    [Tooltip("Sprite when facing down")]  public Sprite downSprite;
    [Tooltip("Sprite when facing left")]  public Sprite leftSprite;
    [Tooltip("Sprite when facing right")] public Sprite rightSprite;

    [Header("Collision")]
    [Tooltip("Which layers block movement")] public LayerMask collisionLayers;
    [Tooltip("Distance to bump back on collision if completely blocked")] public float bumpBackDistance = 0.1f;

    private Rigidbody2D     rb;
    private Animator        animator;
    private SpriteRenderer  spriteRenderer;

    private Vector2 targetPosition;
    private Vector2 moveDir;
    private bool    isMoving;

    // Filter and buffer for Rigidbody2D.Cast
    private ContactFilter2D moveFilter;
    private RaycastHit2D[]  hitBuffer = new RaycastHit2D[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        moveFilter = new ContactFilter2D();
        moveFilter.SetLayerMask(collisionLayers);
        moveFilter.useTriggers = false;

        targetPosition = rb.position;
        isMoving       = false;
    }

    void Update()
    {
        // Pause handling
        if (PauseManager.IsPaused())
        {
            StopMovement();
            return;
        }

        var input = Singleton.Instance.playerInputManager;
        if (input.fireDown)
        {
            targetPosition = input.mousePosWorldSpace;
            Vector2 delta = targetPosition - rb.position;
            if (delta.sqrMagnitude > stoppingDistance * stoppingDistance)
            {
                moveDir = delta.normalized;
                isMoving = true;
                UpdateSprite(moveDir);
                animator.SetBool("isWalking", true);
            }
        }
    }

    void FixedUpdate()
    {
        if (PauseManager.IsPaused() || !isMoving)
        {
            StopMovement();
            return;
        }

        Vector2 toTarget = targetPosition - rb.position;
        float   distToTarget = toTarget.magnitude;

        if (distToTarget <= stoppingDistance)
        {
            StopMovement();
            return;
        }

        // Always drive toward target
        moveDir = toTarget.normalized;
        float moveDist = speed * Time.fixedDeltaTime;

        // Shape-cast ahead to detect walls
        int hitCount = rb.Cast(moveDir, moveFilter, hitBuffer, moveDist + bumpBackDistance);
        if (hitCount > 0)
        {
            Vector2 normal = hitBuffer[0].normal;
            // project moveDir onto tangent to slide along wall
            Vector2 tangent = moveDir - normal * Vector2.Dot(moveDir, normal);
            if (tangent.sqrMagnitude > 0.001f)
            {
                moveDir = tangent.normalized;
            }
            else
            {
                // no tangent (head-on), bump back slightly and stop
                rb.position -= normal * bumpBackDistance;
                StopMovement();
                return;
            }
        }

        // Apply velocity
        rb.linearVelocity = moveDir * speed;
        animator.SetBool("isWalking", true);
        UpdateSprite(moveDir);
    }

    private void StopMovement()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);
    }

    private void UpdateSprite(Vector2 dir)
    {
        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);

        if (ax > ay)
            spriteRenderer.sprite = dir.x > 0 ? rightSprite : leftSprite;
        else
            spriteRenderer.sprite = dir.y > 0 ? upSprite    : downSprite;
    }
}
