using UnityEngine;

public class OverworldNPC : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second")]
    public float speed = 5f;

    [Header("Direction Sprites")]
    [Tooltip("Sprite when facing up")]    public Sprite upSprite;
    [Tooltip("Sprite when facing down")]  public Sprite downSprite;
    [Tooltip("Sprite when facing left")]  public Sprite leftSprite;
    [Tooltip("Sprite when facing right")] public Sprite rightSprite;

    [Header("Collision")]
    [Tooltip("Which layers to stop on collision")] public LayerMask collisionLayers;
    [Tooltip("Distance to bump back on collision")] public float bumpBackDistance = 0.5f;

    public Rigidbody2D rb;
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 targetPosition;
    private Vector2 moveDir;
    public bool isMoving = false;

    public void InitializeFromDialogueCharacter(DialogueCharacter dialogueCharacter)
    {
        upSprite = dialogueCharacter.upSprite;
        downSprite = dialogueCharacter.downSprite;
        leftSprite = dialogueCharacter.leftSprite;
        rightSprite = dialogueCharacter.rightSprite;
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        targetPosition = rb.position;
        moveDir = Vector2.zero;
    }

    void Update()
    {
        /*if (PauseManager.IsPaused())
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }*/

        if (isMoving)
        {
            float remaining = Vector2.Distance(rb.position, targetPosition);
            if (remaining < 0.1f)
            {
                StopMovement();
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
            rb.linearVelocity = moveDir * speed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // only respond to specified layers
        if ((collisionLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            // bump back slightly to avoid sticking through
            rb.position = rb.position - moveDir * bumpBackDistance;
            StopMovement();
        }
    }

    private void StopMovement()
    {
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);
    }

    private void UpdateSprite(Vector2 dir)
    {
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        if (absX > absY)
            spriteRenderer.sprite = dir.x > 0 ? rightSprite : leftSprite;
        else
            spriteRenderer.sprite = dir.y > 0 ? upSprite    : downSprite;
    }

    public void SetTargetPosition(Vector2 targetPos)
    {
        targetPosition = targetPos;
        Vector2 delta = targetPosition - rb.position;
        if (delta.sqrMagnitude > 0.001f)
        {
            moveDir = delta.normalized;
            isMoving = true;
            UpdateSprite(moveDir);
            animator.SetBool("isWalking", true);
        }
    }
}
