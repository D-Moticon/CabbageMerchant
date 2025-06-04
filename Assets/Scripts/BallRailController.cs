using UnityEngine;
using FMOD.Studio;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class BallRailController : MonoBehaviour
{
    [Header("Speed clamp (units/s)")]
    public float minSpeed = 8f;
    public float maxSpeed = 30f;

    [Header("Raycast margin (m)")]
    public float skin = 0.02f;
    
    [Header("Multi‐raycast settings")]
    [Tooltip("How many raycasts to fire each frame when grinding.")]
    public int    raycastSamples        = 3;
    [Tooltip("Distance between successive raycast origins in the velocity direction.")]
    public float  sampleForwardSpacing  = 0.05f;

    [Header("Debug gizmos")]
    public bool showDebugLines = true;

    [Header("VFX & SFX")]
    public PooledObjectData grindVFX;
    public ParticleSystem sparkParticles;
    public SFXInfo grindSFX;

    [Header("Grinding Bonker")]
    public Bonker grindBonker;

    private Ball             b;
    private Rigidbody2D      rb;
    private CircleCollider2D col;

    private bool             grinding;
    private Vector2          railNormal;
    private float            dirSign;
    private float            speed;

    private float            savedGravity;
    private GameObject       vfxInstance;
    private EventInstance    sfxInstance;

    // The rail we’re currently grinding on
    private GrindRail        currentRail;
    // And its specific collider we’ll ignore
    private Collider2D       currentRailCollider;

    // for delayed cleanup
    private Coroutine        endRoutine;

    void Awake()
    {
        b   = GetComponent<Ball>();
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        sparkParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        grindBonker.gameObject.SetActive(false);
        savedGravity      = rb.gravityScale;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        // Only start on rails
        var newRail = c.collider.GetComponent<GrindRail>();
        if (newRail == null) return;

        // Cancel any pending exit cleanup (and kill old VFX/SFX)
        CancelEndRoutine();

        // If we were already on a rail, destroy that one first
        if (grinding && currentRail != null)
        {
            currentRail.HandleDestruction(rb.position);
            PerformCleanup();
        }

        // Now begin grinding the new one
        currentRail         = newRail;
        currentRailCollider = c.collider;
        railNormal          = c.GetContact(0).normal.normalized;
        Vector2 hitPt       = c.GetContact(0).point;

        // Figure out CW vs CCW direction
        Vector2 tCW  = new Vector2(-railNormal.y,  railNormal.x);
        Vector2 tCCW = new Vector2( railNormal.y, -railNormal.x);
        dirSign      = Vector2.Dot(rb.linearVelocity, tCW) >=
                       Vector2.Dot(rb.linearVelocity, tCCW) ? +1f : -1f;

        speed = Mathf.Clamp(rb.linearVelocity.magnitude, minSpeed, maxSpeed);

        // temporarily disable gravity & rotation
        
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;

        // ignore collision just between this ball and that one rail collider
        Physics2D.IgnoreCollision(col, currentRailCollider, true);

        // snap onto rail + drive
        Vector2 firstTangent = (dirSign > 0 ? tCW : tCCW).normalized;
        rb.position       = hitPt + railNormal * col.radius;
        rb.linearVelocity = firstTangent * speed;

        // spawn VFX/SFX/bonker
        if (grindVFX != null)    vfxInstance = grindVFX.Spawn(hitPt);
        if (grindSFX != null)    sfxInstance = grindSFX.Play(10f);
        sparkParticles.Play();
        grindBonker.gameObject.SetActive(true);

        b.gameObject.layer = LayerMask.NameToLayer("BallWallsOnly");
        grinding = true;
    }

    void Update()
    {
        if (!grinding) return;
        if (Singleton.Instance.pauseManager.isPaused) return;

        // Prepare for multi‐raycast
        Vector2 velDir        = rb.linearVelocity.normalized;
        float   len           = col.radius + skin * 3f;
        int     layerMask     = 1 << LayerMask.NameToLayer("Rail");

        Vector2 sumNormal     = Vector2.zero;
        Vector2 sumPoint      = Vector2.zero;
        int     hitCount      = 0;

        // base origin is pushed out by the current normal
        Vector2 baseOrigin    = rb.position + railNormal * skin;

        // fire N raycasts along the velocity direction
        for (int i = 0; i < raycastSamples; i++)
        {
            Vector2 origin = baseOrigin + velDir * (sampleForwardSpacing * i);
            if (showDebugLines)
                Debug.DrawLine(origin, origin - railNormal * len, Color.yellow);

            RaycastHit2D hit = Physics2D.Raycast(origin, -railNormal, len, layerMask);
            if (!hit) continue;

            sumNormal += hit.normal;
            sumPoint  += hit.point;
            hitCount++;

            if (showDebugLines)
            {
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red);
                // draw a small tangent indicator
                Vector2 t = new Vector2(-hit.normal.y, hit.normal.x).normalized * 0.5f * dirSign;
                Debug.DrawLine(hit.point, hit.point + t, Color.cyan);
            }
        }

        // if none of the rays hit, exit grinding
        if (hitCount == 0)
        {
            if (currentRail != null)
            {
                currentRail.HandleDestruction(rb.position);
                currentRail = null;
            }
            ScheduleEndRoutine();
            return;
        }

        // average the normals and points
        railNormal = (sumNormal / hitCount).normalized;
        Vector2 avgHitPoint = sumPoint / hitCount;

        // re‐compute tangent
        Vector2 tangent = (dirSign > 0
                          ? new Vector2(-railNormal.y,  railNormal.x)
                          : new Vector2( railNormal.y, -railNormal.x))
                          .normalized;

        // move VFX
        if (vfxInstance != null)
            vfxInstance.transform.position = avgHitPoint;

        // snap & slide
        rb.position       = avgHitPoint + railNormal * col.radius;
        rb.linearVelocity = tangent * speed;
    }


    private void ScheduleEndRoutine()
    {
        // only start one cleanup coroutine—never cancel it here
        if (endRoutine == null)
            endRoutine = StartCoroutine(EndGrindDelayed());
    }

    private void CancelEndRoutine()
    {
        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
        }

        // kill any lingering VFX
        if (vfxInstance != null)
        {
            vfxInstance.SetActive(false);
            vfxInstance = null;
        }

        // kill any lingering SFX
        if (sfxInstance.isValid())
        {
            sfxInstance.stop(STOP_MODE.IMMEDIATE);
        }
    }

    private IEnumerator EndGrindDelayed()
    {
        yield return new WaitForSeconds(0.05f);
        PerformCleanup();
        endRoutine = null;
    }

    private void PerformCleanup()
    {
        if (!grinding) return;
        
        grinding          = false;
        rb.gravityScale   = savedGravity;
        rb.freezeRotation = false;

        // restore collision with that specific rail
        if (currentRailCollider != null)
        {
            Physics2D.IgnoreCollision(col, currentRailCollider, false);
            currentRailCollider = null;
        }

        // despawn VFX
        if (vfxInstance != null)
        {
            vfxInstance.SetActive(false);
            vfxInstance = null;
        }

        // stop SFX
        if (sfxInstance.isValid())
            sfxInstance.stop(STOP_MODE.IMMEDIATE);

        sparkParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        grindBonker.gameObject.SetActive(false);

        b.gameObject.layer = LayerMask.NameToLayer("Ball");
    }
}
