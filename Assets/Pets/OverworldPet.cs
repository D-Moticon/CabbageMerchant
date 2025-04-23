using UnityEngine;

/// <summary>
/// Pet controller that follows or wanders around the player in a top-down overworld.
/// Trails behind the character in the direction they last moved, uses a constant follow distance,
/// swaps sprites/animations based on movement direction defined in the PetDefinition,
/// stops moving when hovered, and implements IHoverable by delegating to its in-game item.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class OverworldPet : MonoBehaviour, IHoverable
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second")] public float speed = 4f;

    [Header("Follow Settings")]
    [Tooltip("Distance to trail behind the player")] public float followDistance = 0.5f;

    [Header("Wander Settings")]
    [Tooltip("Radius around the player to pick random wander targets")] public float wanderRadius = 2f;
    [Tooltip("Time between picking new wander targets")]      public float wanderInterval = 3f;

    [Header("Pet Definition")]
    [Tooltip("ScriptableObject defining pet visuals and parameters")] public PetDefinition def;

    // runtime state
    private Transform      player;
    private Animator       animator;
    private SpriteRenderer spriteRenderer;
    private Vector3        targetPosition;
    private float          wanderTimer;
    private Vector3        followDir;
    private Vector3        lastPlayerPos;

    private enum State { Follow, Wander, Stop }
    private State current;

    void Awake()
    {
        animator       = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        var charObj = FindObjectOfType<OverworldCharacter>();
        if (charObj)
        {
            player        = charObj.transform;
            lastPlayerPos = player.position;
            followDir     = Vector3.down;
        }

        current = State.Follow;
        targetPosition = player != null
            ? player.position - followDir * followDistance
            : transform.position;
        wanderTimer = wanderInterval;
    }

    void OnEnable()
    {
        // register this pet with the PetManager
        Singleton.Instance.petManager.RegisterOverworldPet(this);
    }

    void OnDisable()
    {
        Singleton.Instance.petManager.UnregisterOverworldPet(this);
    }

    void Update()
    {
        // update trailing direction based on player movement
        if (player != null)
        {
            Vector3 currPos = player.position;
            Vector3 delta   = currPos - lastPlayerPos;
            if (delta.sqrMagnitude > 0.0001f)
            {
                followDir     = delta.normalized;
                lastPlayerPos = currPos;
            }
        }

        // execute behavior based on state
        if (current == State.Follow)
        {
            Follow();
        }
        else if (current == State.Wander)
        {
            Wander();
        }
        // Stop state: do nothing

        // animation
        bool walking = (transform.position - targetPosition).sqrMagnitude > 0.1f && current != State.Stop;
        animator.SetBool("isWalking", walking);

        // sprite direction
        UpdateSpriteDirection();
    }

    public void SetFollow()
    {
        current = State.Follow;
        if (player != null)
            targetPosition = player.position - followDir * followDistance;
    }

    public void SetWander()
    {
        current = State.Wander;
        wanderTimer = 0f;
    }

    public void SetStop()
    {
        current = State.Stop;
    }

    private void Follow()
    {
        if (player == null) return;
        targetPosition = player.position - followDir * followDistance;
        MoveTowardTarget();
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f && player != null)
        {
            Vector2 rnd = Random.insideUnitCircle * wanderRadius;
            targetPosition = player.position + new Vector3(rnd.x, rnd.y, 0f);
            wanderTimer    = wanderInterval;
        }
        MoveTowardTarget();
    }

    private void MoveTowardTarget()
    {
        Vector3 diff = targetPosition - transform.position;
        float   dist = diff.magnitude;
        if (dist < 0.05f) return;
        Vector3 dir  = diff.normalized;
        float   step = speed * Time.deltaTime;
        transform.position += (step < dist ? dir * step : diff);
    }

    private void UpdateSpriteDirection()
    {
        Vector3 moveVec = targetPosition - transform.position;
        if (moveVec.sqrMagnitude < 0.0001f) return;
        float absX = Mathf.Abs(moveVec.x), absY = Mathf.Abs(moveVec.y);
        if (absX > absY)
            spriteRenderer.sprite = moveVec.x > 0 ? def.rightSprite : def.leftSprite;
        else
            spriteRenderer.sprite = moveVec.y > 0 ? def.upSprite    : def.downSprite;
    }

    void OnMouseDown()
    {
        // click to set active pet
        Singleton.Instance.petManager.SetCurrentPet(def);
        SetFollow();
    }

    void OnMouseEnter()
    {
        // immediately stop when hovered
        SetStop();
    }

    void OnMouseExit()
    {
        // resume follow if active pet, otherwise wander
        if (def == Singleton.Instance.petManager.currentPet)
            SetFollow();
        else
            SetWander();
    }

    //=============== IHoverable ===============
    public string GetTitleText(HoverableModifier mod = null)
        => def.itemPrefab.GetTitleText(mod);
    public string GetDescriptionText(HoverableModifier mod = null)
        => def.itemPrefab.GetDescriptionText(mod);
    public string GetTypeText(HoverableModifier mod = null)
        => def.itemPrefab.GetTypeText(mod);
    public string GetRarityText()
        => def.itemPrefab.GetRarityText();
    public string GetTriggerText()
        => def.itemPrefab.GetTriggerText();
    public Sprite GetImage()
        => def.itemPrefab.GetImage();
    public string GetValueText()
        => def.itemPrefab.GetValueText();
}
