using UnityEngine;

/// <summary>
/// Simple top-down RPG character controller driven by mouse clicks.
/// Click (fireDown) to set a targetPosition, and the character will
/// move towards it at the given speed. Uses Animator "isWalking" bool
/// and swaps sprites based on movement direction.
/// </summary>
public class OverworldCharacter : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second")] public float speed = 5f;

    [Header("Direction Sprites")]
    [Tooltip("Sprite when facing up")]    public Sprite upSprite;
    [Tooltip("Sprite when facing down")]  public Sprite downSprite;
    [Tooltip("Sprite when facing left")]  public Sprite leftSprite;
    [Tooltip("Sprite when facing right")] public Sprite rightSprite;

    // runtime references
    private Vector3 targetPosition;
    private bool    isMoving = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        animator = GetComponent<Animator>();
        // find the SpriteRenderer on a child rather than requiring it here
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // initialize target at current position
        targetPosition = transform.position;
    }

    void Update()
    {
        var input = Singleton.Instance.playerInputManager;

        // On click (fireDown), set a new target position
        if (input.fireDown)
        {
            targetPosition = input.mousePosWorldSpace;
            isMoving = true;
        }

        if (isMoving)
        {
            // direction vector toward target
            Vector3 dir = (targetPosition - transform.position);
            float distance = dir.magnitude;

            if (distance < 0.01f)
            {
                // reached target
                isMoving = false;
                animator.SetBool("isWalking", false);
                return;
            }

            dir.Normalize();
            // move step
            float moveStep = speed * Time.deltaTime;
            transform.position += (moveStep < distance)
                ? dir * moveStep
                : dir * distance;

            // update walking animation
            animator.SetBool("isWalking", true);

            // update sprite based on dir
            float absX = Mathf.Abs(dir.x);
            float absY = Mathf.Abs(dir.y);
            if (absX > absY)
            {
                // left or right
                spriteRenderer.sprite = (dir.x > 0) ? rightSprite : leftSprite;
            }
            else
            {
                // up or down
                spriteRenderer.sprite = (dir.y > 0) ? upSprite : downSprite;
            }
        }
        else
        {
            // ensure animation is idle
            animator.SetBool("isWalking", false);
        }
    }
}
