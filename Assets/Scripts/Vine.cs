using UnityEngine;
using System.Collections;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class Vine : MonoBehaviour
{
    private SpriteShapeController shapeController;
    private Spline spline;

    [Header("Draw Point FX")]
    [Tooltip("VFX to spawn at each draw point interval.")]
    public PooledObjectData pointVFX;
    public float pointVFXScale = 1f;
    [Tooltip("SFX to play at each draw point interval.")]
    public SFXInfo pointSFX;
    [Tooltip("Time between successive point FX spawns while drawing (seconds)")]
    public float pointInterval = 0.1f;

    [Header("Retract Point FX")]
    [Tooltip("VFX to spawn when retracting each point.")]
    public PooledObjectData removePointVFX;
    public float removeVFXScale = 1f;
    [Tooltip("SFX to play when retracting each point.")]
    public SFXInfo removePointSFX;

    private float drawTimer;
    private float lifeTimer;
    private float maxDrawDuration;
    private float lifeDuration;
    private bool isDrawing;
    private bool hasRetractionStarted;

    private const float MinPointDistance = 0.1f;
    private float intervalTimer;

    public Material normalMat;
    public Material rainbowMat;

    void Awake()
    {
        shapeController = GetComponent<SpriteShapeController>();
        spline = shapeController.spline;
    }

    void OnEnable()
    {
        GameStateMachine.ExitingBounceStateAction += OnBounceStateExited;
    }
    void OnDisable()
    {
        GameStateMachine.ExitingBounceStateAction -= OnBounceStateExited;
    }

    /// <summary>
    /// Begin drawing for drawDuration, then persist for lifeDuration before retract.
    /// </summary>
    public void Initialize(float drawDuration, float lifeDur)
    {
        maxDrawDuration = drawDuration;
        lifeDuration    = lifeDur;
        drawTimer       = 0f;
        lifeTimer       = 0f;
        intervalTimer   = 0f;
        isDrawing       = true;
        hasRetractionStarted = false;

        spline.Clear();
        StartCoroutine(DrawRoutine());
    }

    private IEnumerator DrawRoutine()
    {
        AddPointAtPointer();

        while (isDrawing && drawTimer < maxDrawDuration && Singleton.Instance.playerInputManager.weaponFireHeld)
        {
            float dt = Time.deltaTime;
            drawTimer     += dt;
            intervalTimer += dt;

            AddPointAtPointer();

            if (intervalTimer >= pointInterval)
            {
                intervalTimer = 0f;
                Vector3 lastPos = spline.GetPosition(spline.GetPointCount() - 1);
                GameObject vfx = pointVFX?.Spawn(lastPos);
                vfx.transform.localScale = new Vector3(pointVFXScale, pointVFXScale, pointVFXScale);
                pointSFX?.Play();
            }
            yield return null;
        }

        isDrawing = false;
        // start life countdown
        StartCoroutine(LifetimeRoutine());
    }

    private void AddPointAtPointer()
    {
        Vector2 p = Singleton.Instance.playerInputManager.mousePosWorldSpace;
        int count = spline.GetPointCount();
        if (count == 0)
            spline.InsertPointAt(0, p);
        else
        {
            Vector2 last = spline.GetPosition(count - 1);
            if (Vector2.Distance(last, p) > MinPointDistance)
                spline.InsertPointAt(count, p);
        }
        shapeController.BakeCollider();
    }

    private IEnumerator LifetimeRoutine()
    {
        while (lifeTimer < lifeDuration)
        {
            lifeTimer += Time.deltaTime;
            yield return null;
        }
        TryStartRetraction();
    }

    private void OnBounceStateExited()
    {
        TryStartRetraction();
    }

    private void TryStartRetraction()
    {
        if (hasRetractionStarted)
            return;
        hasRetractionStarted = true;

        StopAllCoroutines();       // stop Draw & Lifetime
        StartCoroutine(RetractRoutine());
    }

    private IEnumerator RetractRoutine()
    {
        int total = spline.GetPointCount();
        if (total == 0)
        {
            gameObject.SetActive(false);
            yield break;
        }
        float retractDuration = maxDrawDuration * 0.5f;
        float interval        = retractDuration / total;

        for (int i = 0; i < total; i++)
        {
            Vector3 pos = spline.GetPosition(0);
            GameObject vfx = removePointVFX?.Spawn(pos);
            vfx.transform.localScale = new Vector3(pointVFXScale, pointVFXScale, pointVFXScale);
            removePointSFX?.Play();

            spline.RemovePointAt(0);
            shapeController.BakeCollider();

            yield return new WaitForSeconds(interval);
        }

        gameObject.SetActive(false);
    }

    public void MakeRainbow()
    {
        var spr = GetComponent<SpriteShapeRenderer>();
        var mats = spr.sharedMaterials;  
        mats[1] = rainbowMat;     
        spr.sharedMaterials = mats;
    }

    public void MakeNonRainbow()
    {
        var spr = GetComponent<SpriteShapeRenderer>();
        var mats = spr.sharedMaterials;  
        mats[1] = normalMat;     
        spr.sharedMaterials = mats;
    }
}
