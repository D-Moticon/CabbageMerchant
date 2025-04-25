using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Pet controller that follows or wanders around the player in a top-down overworld.
/// Trails behind the character in the direction they last moved, uses a constant follow distance,
/// swaps sprites/animations based on movement direction defined in the PetDefinition,
/// stops moving when hovered, and implements IHoverable by delegating to its in-game item.
/// Supports wander bounds, avoids overlapping other pets, and skips Default-layer colliders.
/// Randomizes wander interval each move.
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
    [Tooltip("Minimum time between moves")]
    public float wanderIntervalMin = 1f;
    [Tooltip("Maximum time between moves")]
    public float wanderIntervalMax = 3f;

    [Header("Wander Bounds")]
    [Tooltip("Minimum X position allowed")] public float minX = -10f;
    [Tooltip("Maximum X position allowed")] public float maxX = 10f;
    [Tooltip("Minimum Y position allowed")] public float minY = -5f;
    [Tooltip("Maximum Y position allowed")] public float maxY = 5f;
    [Tooltip("Minimum allowed distance between pets")] public float minPetSeparation = 0.5f;

    [Header("Pet Definition")]
    [Tooltip("ScriptableObject defining pet visuals")] public PetDefinition def;

    // runtime state
    private Transform      player;
    public Animator       animator;
    public SpriteRenderer spriteRenderer;
    private Vector3        targetPosition;
    private float          wanderTimer;
    private Vector3        followDir;
    private Vector3        lastPlayerPos;

    private enum State { Follow, Wander, Stop }
    private State current;

    public SpriteRenderer currentPetIndicator;
    
    void Awake()
    {
        var charObj = FindFirstObjectByType<OverworldCharacter>();
        if (charObj)
        {
            player        = charObj.transform;
            lastPlayerPos = player.position;
            followDir     = Vector3.down;
        }

        current = State.Follow;
        targetPosition = player != null
            ? ClampToBounds(player.position - followDir * followDistance)
            : transform.position;
        // randomize initial timer
        wanderTimer = Random.Range(wanderIntervalMin, wanderIntervalMax);
    }

    void OnEnable()
    {
        Singleton.Instance.petManager.RegisterOverworldPet(this);
    }

    void OnDisable()
    {
        Singleton.Instance.petManager.UnregisterOverworldPet(this);
    }


    public void Initialize(PetDefinition pd)
    {
        def = pd;
        spriteRenderer.sprite = pd.downSprite;
    }
    void Update()
    {
        if (PauseManager.IsPaused())
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (player != null)
        {
            var currPos = player.position;
            var delta   = currPos - lastPlayerPos;
            if (delta.sqrMagnitude > 0.0001f)
            {
                followDir     = delta.normalized;
                lastPlayerPos = currPos;
            }
        }

        switch (current)
        {
            case State.Follow: Follow(); break;
            case State.Wander: Wander(); break;
            case State.Stop: break;
        }

        bool walking = (transform.position - targetPosition).sqrMagnitude > 0.1f && current != State.Stop;
        animator.SetBool("isWalking", walking);

        UpdateSpriteDirection();
    }

    public void SetFollow()
    {
        current = State.Follow;
        if (player != null)
            targetPosition = ClampToBounds(player.position - followDir * followDistance);
        currentPetIndicator.enabled = true;
    }

    public void SetWander()
    {
        current = State.Wander;
        wanderTimer = 0f;
        currentPetIndicator.enabled = false;
    }

    public void SetStop()
    {
        current = State.Stop;
    }

    private void Follow()
    {
        if (player == null) return;
        targetPosition = ClampToBounds(player.position - followDir * followDistance);
        MoveTowardTarget();
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f && player != null)
        {
            Vector3 candidate = Vector3.zero;
            int attempts = 0;
            do
            {
                var rnd = Random.insideUnitCircle * wanderRadius;
                candidate = player.position + new Vector3(rnd.x, rnd.y, 0f);
                candidate = ClampToBounds(candidate);
                attempts++;
                if (attempts > 10) break;
            }
            while (IsOverlappingOtherPets(candidate) || OverlapsDefaultLayer(candidate));

            targetPosition = candidate;
            // randomize next interval
            wanderTimer = Random.Range(wanderIntervalMin, wanderIntervalMax);
        }
        MoveTowardTarget();
    }

    private void MoveTowardTarget()
    {
        var diff = targetPosition - transform.position;
        var dist = diff.magnitude;
        if (dist < 0.05f) return;
        var dir  = diff.normalized;
        var step = speed * Time.deltaTime;
        transform.position += (step < dist ? dir * step : diff);
    }

    private void UpdateSpriteDirection()
    {
        var mv = targetPosition - transform.position;
        if (mv.sqrMagnitude < 0.0001f) return;
        var ax = Mathf.Abs(mv.x);
        var ay = Mathf.Abs(mv.y);
        if (ax > ay)
            spriteRenderer.sprite = mv.x > 0 ? def.rightSprite : def.leftSprite;
        else
            spriteRenderer.sprite = mv.y > 0 ? def.upSprite : def.downSprite;
    }

    void OnMouseDown()
    {
        if (PauseManager.IsPaused())
        {
            return;
        }
        Singleton.Instance.petManager.SetCurrentPet(def);
        SetFollow();
    }

    void OnMouseEnter() => SetStop();
    void OnMouseExit()
    {
        if (def == Singleton.Instance.petManager.currentPet)
            SetFollow();
        else
            SetWander();
    }

    private Vector3 ClampToBounds(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

    private bool IsOverlappingOtherPets(Vector3 candidate)
    {
        foreach (var other in Singleton.Instance.petManager.overworldPets)
        {
            if (other == this) continue;
            if (Vector3.Distance(candidate, other.transform.position) < minPetSeparation)
                return true;
        }
        return false;
    }

    private bool OverlapsDefaultLayer(Vector3 candidate)
    {
        var radius = minPetSeparation;
        int mask = 1 << LayerMask.NameToLayer("Default");
        return Physics2D.OverlapCircle(candidate, radius, mask) != null;
    }

    //=============== IHoverable ===============
    public string GetTitleText(HoverableModifier mod = null) => def.itemPrefab.GetTitleText(mod);
    public string GetDescriptionText(HoverableModifier mod = null) => def.itemPrefab.GetDescriptionText(mod);
    public string GetTypeText(HoverableModifier mod = null) => def.itemPrefab.GetTypeText(mod);
    public string GetRarityText() => def.itemPrefab.GetRarityText();
    public string GetTriggerText() => def.itemPrefab.GetTriggerText();
    public Sprite GetImage() => def.itemPrefab.GetImage();
    public string GetValueText() => def.itemPrefab.GetValueText();
}
