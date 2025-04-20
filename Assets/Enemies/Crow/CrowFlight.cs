using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class CrowFlight : MonoBehaviour, IKillable
{
    public enum TargetMode { nearestCabbage, randomCabbage }

    [Header("Flight Speeds")]
    [Tooltip("Speed when flying empty-handed toward cabbage")]
    public float emptyFlightSpeed = 2f;
    [Tooltip("Speed when flying upward carrying cabbage")]
    public float cabbageFlightSpeed = 10f;

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.nearestCabbage;

    [Header("Grab Settings")]
    [Tooltip("Distance at which crow will grab the cabbage (world units)")]
    public float grabRadius = 0.35f;

    public float grabYOffset = 0.5f;
    public Transform grabParent;
    
    [Header("Pause Settings")]
    [Tooltip("Whether crow should pause during player aiming phase")]
    public bool pauseDuringAim = true;
    private bool isPaused = false;

    private Rigidbody2D rb;
    private Cabbage targetCabbage;
    private bool hasCabbage = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        GameStateMachine.EnteringAimStateAction += OnAimStateEntered;
        GameStateMachine.BallFiredEvent += OnBallFired;

        if (pauseDuringAim)
            isPaused = true;
    }

    private void OnDisable()
    {
        // Unsubscribe events only; cabbage release is handled in Kill()
        GameStateMachine.EnteringAimStateAction -= OnAimStateEntered;
        GameStateMachine.BallFiredEvent -= OnBallFired;
    }

    void Start()
    {
        ChooseTarget();
    }

    void Update()
    {
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!hasCabbage)
        {
            if (targetCabbage == null || !targetCabbage.gameObject.activeInHierarchy || !targetCabbage.enabled)
            {
                ChooseTarget();
                if (targetCabbage == null)
                {
                    rb.linearVelocity = Vector2.zero;
                    return;
                }
            }

            Vector2 dir = (targetCabbage.transform.position - transform.position).normalized;
            rb.linearVelocity = dir * emptyFlightSpeed;

            float dist = Vector2.Distance(transform.position, targetCabbage.transform.position);
            if (dist <= grabRadius)
                StealCabbage(targetCabbage);
        }
        else
        {
            rb.linearVelocity = Vector2.up * cabbageFlightSpeed;
        }
    }

    private void OnAimStateEntered()
    {
        if (pauseDuringAim)
            isPaused = true;
    }

    private void OnBallFired(Ball ball)
    {
        isPaused = false;
    }

    private void ChooseTarget()
    {
        var all = FindObjectsOfType<Cabbage>()
            .Where(c => c.gameObject.activeInHierarchy && c.enabled && !c.isStolen)
            .ToArray();
        if (all.Length == 0)
        {
            targetCabbage = null;
            return;
        }

        if (targetMode == TargetMode.randomCabbage)
            targetCabbage = all[Random.Range(0, all.Length)];
        else
        {
            float min = float.MaxValue;
            Cabbage nearest = null;
            foreach (var c in all)
            {
                float d = (c.transform.position - transform.position).sqrMagnitude;
                if (d < min)
                {
                    min = d;
                    nearest = c;
                }
            }
            targetCabbage = nearest;
        }
    }

    private void StealCabbage(Cabbage cabbage)
    {
        hasCabbage = true;
        cabbage.isStolen = true;

        var cabRb = cabbage.GetComponent<Rigidbody2D>();
        if (cabRb != null) cabRb.simulated = false;
        var cabCol = cabbage.GetComponent<Collider2D>();
        if (cabCol != null) cabCol.enabled = false;

        cabbage.transform.SetParent(grabParent);
        cabbage.transform.localPosition = Vector2.zero - Vector2.up*grabYOffset;

        rb.linearVelocity = Vector2.up * cabbageFlightSpeed;
    }

    /// <summary>
    /// Call this to kill (deactivate) the crow.
    /// This will release any stolen cabbage before deactivation.
    /// </summary>
    public void Kill()
    {
        if (hasCabbage && targetCabbage != null)
        {
            // release cabbage
            targetCabbage.transform.SetParent(null);
            var cabRb = targetCabbage.GetComponent<Rigidbody2D>();
            if (cabRb != null) cabRb.simulated = true;
            var cabCol = targetCabbage.GetComponent<Collider2D>();
            if (cabCol != null) cabCol.enabled = true;
            targetCabbage.isStolen = false;
        }
        // deactivate crow
        gameObject.SetActive(false);
    }
}
