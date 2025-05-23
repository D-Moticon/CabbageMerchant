using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class OverworldCharacter : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in world units per second")]    public float speed = 5f;
    [Tooltip("How close to the target before we consider ourselves arrived")] public float stoppingDistance = 0.1f;

    public bool isPlayer = false;
    
    [Header("Direction Sprites")]
    Skin currentSkin;
    private NavMeshAgent agent;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        SkinManager.SkinEquippedEvent += SkinEquippedListener;
        
    }

    private void OnDisable()
    {
        SkinManager.SkinEquippedEvent -= SkinEquippedListener;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (isPlayer)
        {
            SetSkin(Singleton.Instance.skinManager.currentSkin);
        }
    }

    private void SkinEquippedListener(Skin s)
    {
        SetSkin(s);
    }
    
    public void SetSkin(Skin s)
    {
        currentSkin = s;
        spriteRenderer.sprite = currentSkin.downSprite;
    }
    
    void Update()
    {
        if (PauseManager.IsPaused())
        {
            agent.isStopped = true;
            animator.SetBool("isWalking", false);
            return;
        }

        if (agent.enabled == false)
        {
            return;
        }

        var input = Singleton.Instance.playerInputManager;
        if (input.fireDown)
        {
            Vector2 target2D = input.mousePosWorldSpace;
            SetTargetPosition(target2D);
        }
        
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                animator.SetBool("isWalking", false);
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
            }
            else
            {
                Vector2 dir = agent.velocity.normalized;
                animator.SetBool("isWalking", true);
                UpdateFacing(dir);
            }
        }
    }

    public void ForceStop()
    {
        SetTargetPosition(this.transform.position);
    }
    
    public void SetTargetPosition(Vector2 pos)
    {
        agent.isStopped = false;
        Vector3 dest = new Vector3(pos.x, pos.y, transform.position.z);
        agent.SetDestination(dest);
    }

    private void UpdateFacing(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        float ax = Mathf.Abs(dir.x);
        float ay = Mathf.Abs(dir.y);
        
        if (ax > ay)
            spriteRenderer.sprite = dir.x > 0 ? currentSkin.rightSprite : currentSkin.leftSprite;
        else
            spriteRenderer.sprite = dir.y > 0 ? currentSkin.upSprite : currentSkin.downSprite;
    }
}
