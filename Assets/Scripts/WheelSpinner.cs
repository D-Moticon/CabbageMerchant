using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;

[Serializable]
public class WheelSegment
{
    [Tooltip("Label to display and return on land")] public string label;
    [Tooltip("Color for this segment's text and delimiter line")] public Color color = Color.white;
    [Tooltip("Background fill color for this slice.")] public Color backgroundColor = Color.clear;
    [Tooltip("Relative weight of this segment (proportional slice size)")] public float weight = 1f;
    public string textPrefix = "";
    [SerializeReference]
    public DialogueTask taskOnWin;
}

public class WheelSpinner : MonoBehaviour
{
    [Header("Spin Settings")]
    [Tooltip("Duration of the spin animation (seconds)")]
    public float spinDuration = 3f;
    [Tooltip("Ease curve, evaluates from 1→0 to decelerate")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Minimum full turns before landing")]
    public int minExtraTurns = 2;
    [Tooltip("Maximum full turns before landing")]
    public int maxExtraTurns = 5;

    [Header("Wheel Data")]
    [Tooltip("Transform of your wheel sprite (world‑ or UI‑space)")]
    public Transform wheelTransform;
    [Tooltip("Radius of the wheel for drawing lines/text positions")]
    public float wheelRadius = 1f;
    [Tooltip("Segments in clockwise order around the wheel")]
    public List<WheelSegment> segments = new List<WheelSegment>();

    [Header("Visual Prefabs")]
    [Tooltip("Material used to draw the slice backgrounds.")]
    public Material sliceMaterial;
    [Tooltip("Prefab of a LineRenderer to draw segment delimiters")]
    public LineRenderer segmentLinePrefab;
    [Tooltip("Prefab of a TMP_Text for segment labels")]
    public TMP_Text segmentTextPrefab;
    public Color lineColor = Color.black;
    public float lineInnerPointDist = 0.2f;
    public float lineOuterPointDist = 0.1f;

    [Tooltip("Transform of the arrow object pointing at the wheel")]
    public Transform arrowTransform;
    [Tooltip("Maximum deflection angle of the arrow")]
    public float arrowMaxAngle = 30f;
    [Tooltip("Curve defining arrow motion over one cycle (phase 0→1)")]
    public AnimationCurve arrowOscillationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Multiplier to convert wheel speed into arrow phase speed")]
    public float arrowSpeedMultiplier = 3f;
    
    public ItemCollection randomItemCollection;
    public DialogueLine randomItemLine;

    [SerializeReference]
    public DialogueTask fadeInDialogueTask;
    public SFXInfo wheelSpinSFX;
    public SFXInfo wheelClickSFX;
    public SFXInfo winningSFX;
    public SFXInfo losingSFX;
    public PooledObjectData holofoilVFX;
    public SFXInfo holofoilSFX;

    // slice angles (degrees) and cumulative
    private float[] _sliceAngles;
    private float[] _cumAngles;
    // track spawned visuals for cleanup
    private readonly List<GameObject> _generated = new List<GameObject>();

    private void Awake()
    {
        if (wheelTransform == null) wheelTransform = transform;
        if (segments.Count < 1)
            Debug.LogWarning("WheelSpinner: No segments defined!");

        CalculateSlices();
        BuildWheelVisuals();
    }

    public void AddWeightToSegment(int index, float additionalWeight)
    {
        if (index < 0 || index >= segments.Count)
        {
            Debug.LogWarning($"WheelSpinner: Invalid segment index {index}");
            return;
        }
        segments[index].weight += additionalWeight;
        CalculateSlices();
        BuildWheelVisuals();
    }
    
    private void CalculateSlices()
    {
        float total = segments.Sum(s => s.weight);
        int n = segments.Count;
        _sliceAngles = new float[n];
        _cumAngles = new float[n];
        float acc = 0f;
        for (int i = 0; i < n; i++)
        {
            float angle = (total > 0f) ? (segments[i].weight / total) * 360f : 360f / n;
            _sliceAngles[i] = angle;
            acc += angle;
            _cumAngles[i] = acc;
        }
    }

    /// <summary>
    /// Instantiates line delimiters and labels around the wheel based on weights.
    /// </summary>
    public void BuildWheelVisuals()
    {
        // cleanup old
        foreach (var go in _generated)
            Destroy(go);
        _generated.Clear();

        float startAngle = 0f;
        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            float slice = _sliceAngles[i];
            float angle = startAngle;

            // --- background slice ---
            if (sliceMaterial != null)
            {
                var bg = new GameObject($"SliceBG_{i}");
                bg.transform.SetParent(wheelTransform, false);
                var mf = bg.AddComponent<MeshFilter>();
                var mr = bg.AddComponent<MeshRenderer>();
                var sg = bg.AddComponent<SortingGroup>();
                sg.sortingOrder = -8;
                mr.material = sliceMaterial;
                mr.material.color = seg.backgroundColor;
                mf.mesh = CreateSectorMesh(angle, slice, wheelRadius);
                _generated.Add(bg);
            }

            // --- delimiter line ---
            if (segmentLinePrefab != null)
            {
                var lineGo = Instantiate(segmentLinePrefab.gameObject, wheelTransform);
                lineGo.transform.localPosition = Vector3.zero;
                var lr = lineGo.GetComponent<LineRenderer>();
                lr.startColor = lr.endColor = lineColor;
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                lr.positionCount = 2;
                lr.SetPosition(0, dir * lineInnerPointDist);
                lr.SetPosition(1, dir * (wheelRadius - lineOuterPointDist));
                _generated.Add(lineGo);
            }

            // --- label text ---
            if (segmentTextPrefab != null)
            {
                var txtGo = Instantiate(segmentTextPrefab.gameObject, wheelTransform);
                txtGo.transform.localScale = new Vector3(1f / wheelTransform.localScale.x,
                    1f / wheelTransform.localScale.y, 1f);
                var txt = txtGo.GetComponent<TMP_Text>();
                txt.text = seg.textPrefix + seg.label;
                txt.color = seg.color;

                float midAngle = angle + slice * 0.5f;
                Vector3 dir = Quaternion.Euler(0, 0, midAngle) * Vector3.right;
                txtGo.transform.localPosition = dir * wheelRadius * 0.6f;
                txtGo.transform.rotation = Helpers.Vector2ToRotation(dir);
                _generated.Add(txtGo);
            }

            startAngle += slice;
        }
    }

    private int PickWeightedSegmentIndex()
    {
        float total = segments.Sum(s => s.weight);
        float r = Random.Range(0f, total);
        float acc = 0f;
        for (int i = 0; i < segments.Count; i++)
        {
            acc += segments[i].weight;
            if (r <= acc)
                return i;
        }
        return segments.Count - 1; // fallback
    }
    
    private int GetCurrentSegmentIndex()
    {
        // 1) Direction from wheel center to arrow tip in world space
        Vector3 dir = (arrowTransform.position - wheelTransform.position).normalized;

        // 2) Angle of that vector in world-space degrees [0…360)
        float pointerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (pointerAngle < 0f) pointerAngle += 360f;

        // 3) How much the wheel itself has rotated (also [0…360))
        float wheelAngle = wheelTransform.eulerAngles.z % 360f;
        if (wheelAngle < 0f) wheelAngle += 360f;

        // 4) Angle into the wheel’s coordinate system
        float angleInWheel = pointerAngle - wheelAngle;
        if (angleInWheel < 0f) angleInWheel += 360f;

        // 5) Find which cumulative slice boundary it falls under
        for (int i = 0; i < _cumAngles.Length; i++)
        {
            if (angleInWheel <= _cumAngles[i])
                return i;
        }
        return _cumAngles.Length - 1;
    }
    
    /// <summary>
    /// Spin the wheel: picks a random target rotation and animates.
    /// </summary>
    public IEnumerator SpinRoutine(DialogueContext dc)
    {
        // 1) pick your prize by weight up front
        int selectedIndex = PickWeightedSegmentIndex();

        // 2) figure out that slice’s mid-angle (in wheel-local degrees)
        float sliceStart = (selectedIndex == 0)
            ? 0f
            : _cumAngles[selectedIndex - 1];
        float sliceAngle = _sliceAngles[selectedIndex];
        float midAngle = sliceStart + sliceAngle * 0.5f;

        // 3) compute where the arrow “points” in wheel-local space
        //    (assumes arrowTransform is a child of wheelTransform)
        Vector3 localArrowPos = arrowTransform.localPosition;
        float pointerAngle = Mathf.Atan2(localArrowPos.y, localArrowPos.x) * Mathf.Rad2Deg;
        if (pointerAngle < 0f) pointerAngle += 360f;

        // 4) spin so that midAngle ends up under pointerAngle
        float fromZ = wheelTransform.eulerAngles.z;
        float extraRot = Random.Range(minExtraTurns, maxExtraTurns) * 360f;
        float toZ = fromZ + extraRot + (midAngle - pointerAngle);

        // play SFX
        wheelSpinSFX.Play(spinDuration);

        // 5) animate your spin exactly as before, just replacing `toZ`
        float elapsed = 0f;
        float prevZ = fromZ;
        float arrowPhase = 0f;
        float prevPhase01 = 0f;
        while (elapsed < spinDuration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float mix = ease.Evaluate(t);
            float z = Mathf.Lerp(toZ, fromZ, mix);
            wheelTransform.eulerAngles = new Vector3(0, 0, z);

            // arrow “click” feedback
            float deltaZ = Mathf.DeltaAngle(prevZ, z);
            float wheelSpeed = deltaZ / dt;
            prevZ = z;
            arrowPhase += wheelSpeed * arrowSpeedMultiplier * dt;
            float phase01 = (arrowPhase % 360f) / 360f;
            if (phase01 < prevPhase01) wheelClickSFX.Play();
            prevPhase01 = phase01;

            if (arrowTransform != null)
            {
                float eval = arrowOscillationCurve.Evaluate(phase01);
                float arrowAngle = Mathf.Lerp(-arrowMaxAngle, arrowMaxAngle, eval);
                arrowTransform.localEulerAngles = new Vector3(0, 0, 270f + arrowAngle);
            }

            yield return null;
        }

        // 6) reset arrow as before…
        if (arrowTransform != null)
        {
            float initPhase = (arrowPhase % 360f) / 360f;
            float resetTime = 0.1f;
            float e2 = 0f;
            while (e2 < resetTime)
            {
                e2 += Time.deltaTime;
                float rt = Mathf.Clamp01(e2 / resetTime);
                float phase = Mathf.Lerp(initPhase, 0f, rt);
                float eval = arrowOscillationCurve.Evaluate(phase);
                float arrowAngle = Mathf.Lerp(-arrowMaxAngle, arrowMaxAngle, eval);
                arrowTransform.localEulerAngles = new Vector3(0, 0, 270f + arrowAngle);
                yield return null;
            }
            arrowTransform.localEulerAngles = new Vector3(0, 0, 270f);
            wheelClickSFX.Play();
        }

        // 7) now we already know our winner index
        int index = GetCurrentSegmentIndex();

        // 8) and run your dialogue & prize logic exactly as before:
        var fadeTask = new Task(fadeInDialogueTask.RunTask(dc));
        while (fadeTask.Running) yield return null;

        if (segments[index].taskOnWin != null)
        {
            var winTask = new Task(segments[index].taskOnWin.RunTask(dc));
            while (winTask.Running) yield return null;
        }
    }
    
    private Mesh CreateSectorMesh(float startAngleDeg, float angleDeg, float radius, int steps = 8)
    {
        Mesh m = new Mesh();
        var verts = new List<Vector3> { Vector3.zero };
        var tris  = new List<int>();

        // sample around the arc
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float a = startAngleDeg + angleDeg * t;
            float rad = Mathf.Deg2Rad * a;
            verts.Add(new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius);
            if (i > 0)
            {
                // triangle: center, last, new
                tris.Add(0);
                tris.Add(i);
                tris.Add(i+1);
            }
        }

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        return m;
    }
}
