using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Random = UnityEngine.Random;

[Serializable]
public class WheelSegment
{
    [Tooltip("Label to display and return on land")] public string label;
    [Tooltip("Color for this segment's text and delimiter line")] public Color color = Color.white;
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

    // internal slice size
    private float _sliceSize;
    // track spawned visuals for cleanup
    private readonly List<GameObject> _generated = new List<GameObject>();

    private void Awake()
    {
        if (wheelTransform == null) wheelTransform = transform;
        if (segments.Count < 1)
            Debug.LogWarning("WheelSpinner: No segments defined!");

        _sliceSize = 360f / segments.Count;
        BuildWheelVisuals();
    }

    /// <summary>
    /// Instantiates line delimiters and labels around the wheel.
    /// </summary>
    public void BuildWheelVisuals()
    {
        // cleanup old
        foreach (var go in _generated)
            Destroy(go);
        _generated.Clear();

        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            float angle = i * _sliceSize;

            // --- delimiter line ---
            if (segmentLinePrefab != null)
            {
                var lineGo = Instantiate(segmentLinePrefab.gameObject, wheelTransform);
                lineGo.transform.localPosition = Vector3.zero;
                var lr = lineGo.GetComponent<LineRenderer>();
                lr.startColor = lr.endColor = lineColor;

                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                lr.positionCount = 2;
                lr.SetPosition(0, dir*lineInnerPointDist);
                lr.SetPosition(1, dir * (wheelRadius-lineOuterPointDist));

                _generated.Add(lineGo);
            }

            // --- label text ---
            if (segmentTextPrefab != null)
            {
                var txtGo = Instantiate(segmentTextPrefab.gameObject, wheelTransform);
                txtGo.transform.localScale = new Vector3(1f/wheelTransform.localScale.x, 1f/wheelTransform.localScale.y, 1f);
                var txt = txtGo.GetComponent<TMP_Text>();
                txt.text = seg.textPrefix + seg.label;
                txt.color = seg.color;

                // position at mid‑slice radius * 0.6
                float midAngle = angle + _sliceSize * 0.5f;
                Vector3 dir    = Quaternion.Euler(0, 0, midAngle) * Vector3.right;
                txtGo.transform.localPosition = dir * wheelRadius * 0.6f;

                // ensure upright text
                txtGo.transform.rotation = Helpers.Vector2ToRotation(dir);

                _generated.Add(txtGo);
            }
        }
    }

    /// <summary>
    /// Spin the wheel: picks a random target rotation and animates.
    /// </summary>


    public IEnumerator SpinRoutine(DialogueContext dc)
    {
        float extra   = UnityEngine.Random.Range(minExtraTurns, maxExtraTurns) * 360f;
        float offset  = UnityEngine.Random.Range(0f, 360f);
        float fromZ  = wheelTransform.eulerAngles.z;
        float toZ    = fromZ + extra + offset;
        wheelSpinSFX.Play(spinDuration);
        
        float elapsed = 0f;
        float prevZ   = fromZ;
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

            float deltaZ = Mathf.DeltaAngle(prevZ, z);
            float wheelSpeed = deltaZ / dt;
            prevZ = z;

            arrowPhase += wheelSpeed * arrowSpeedMultiplier * dt;
            float phase01 = (arrowPhase % 360f) / 360f;

            // play tick when wrapping from near 1 back to 0
            if (phase01 < prevPhase01)
            {
                wheelClickSFX.Play();
            }
            prevPhase01 = phase01;

            float arrowEval = arrowOscillationCurve.Evaluate(phase01);
            float arrowAngle = Mathf.Lerp(-arrowMaxAngle, arrowMaxAngle, arrowEval);

            if (arrowTransform != null)
                arrowTransform.localEulerAngles = new Vector3(0, 0, 270f + arrowAngle);

            yield return null;
        }

        float arrowResetDuration = 0.1f;
        
        // after spin, reset arrow using oscillation curve over arrowResetDuration
        if (arrowTransform != null)
        {
            float initialPhase = (arrowPhase % 360f) / 360f;
            float elapsedReset = 0f;
            while (elapsedReset < arrowResetDuration)
            {
                elapsedReset += Time.deltaTime;
                float rt = Mathf.Clamp01(elapsedReset / arrowResetDuration);
                // reverse phase: go from initialPhase down to 0
                float phase = Mathf.Lerp(initialPhase, 0f, rt);
                float eval = arrowOscillationCurve.Evaluate(phase);
                float arrowAngle = Mathf.Lerp(-arrowMaxAngle, arrowMaxAngle, eval);
                arrowTransform.localEulerAngles = new Vector3(0, 0, 270f + arrowAngle);
                yield return null;
            }
            // ensure centered
            arrowTransform.localEulerAngles = new Vector3(0, 0, 270f);
            wheelClickSFX.Play();
        }

        // normalize
        float wheelAngle = wheelTransform.eulerAngles.z % 360f;
        if (wheelAngle < 0) wheelAngle += 360f;
        // arrow points down (world angle = 270°)
        float prizeAngle = 0f;
        float diff = prizeAngle - wheelAngle;
        diff = (diff % 360f + 360f) % 360f;
        int index = Mathf.FloorToInt(diff / _sliceSize);
        index = Mathf.Clamp(index, 0, segments.Count - 1);

        Task fadeTask = new Task(fadeInDialogueTask.RunTask(dc));
        while (fadeTask.Running)
        {
            yield return null;
        }
        
        if (segments[index].taskOnWin != null)
        {
            Task winTask = new Task(segments[index].taskOnWin.RunTask(dc));
            while (winTask.Running)
            {
                yield return null;
            }
        }

        Task prizeTask = null;
        
        
        switch (index)
        {
            case 0:
                prizeTask = new Task(BankruptTask());
                break;
            case 1:
                prizeTask = new Task(MoneyTask(15));
                break;
            case 2:
                prizeTask = new Task(DestroyARandomItemTask());
                break;
            case 3:
                prizeTask = new Task(MakeARandomItemHolofoilTask());
                break;
            case 4:
                prizeTask = new Task(AddLifeTask());
                break;
            case 5:
                prizeTask = new Task(GiveRandomItemTask(dc));
                break;
            default:
                break;
        }
        
        while (prizeTask.Running)
        {
            yield return null;
        }
    }

    IEnumerator BankruptTask()
    {
        Singleton.Instance.playerStats.AddCoins(-Singleton.Instance.playerStats.coins);
        losingSFX.Play();

        yield return new WaitForSeconds(1f);
    }
    
    IEnumerator MoneyTask(int coinAmount)
    {
        Singleton.Instance.playerStats.AddCoins(coinAmount);
        Singleton.Instance.itemManager.sellSFX.Play();
        Singleton.Instance.itemManager.sellVFX.transform.position =
            Singleton.Instance.uiManager.coinsText.transform.position;
        Singleton.Instance.itemManager.sellVFX.Play();

        yield return new WaitForSeconds(1f);
    }
    
    IEnumerator DestroyARandomItemTask()
    {
        List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();

        if (items == null || items.Count == 0)
        {
            yield return new WaitForSeconds(1f);
            yield break;
        }
        
        int rand = Random.Range(0, items.Count);
        Item itemToDestroy = items[rand];
        if(itemToDestroy != null)
        {
            Singleton.Instance.itemManager.DestroyItem(itemToDestroy, true);
        }
        losingSFX.Play();

        yield return new WaitForSeconds(1f);
    }

    IEnumerator MakeARandomItemHolofoilTask()
    {
        List<Item> items = Singleton.Instance.itemManager.GetItemsInInventory();

        if (items == null || items.Count == 0)
        {
            yield return new WaitForSeconds(1f);
            yield break;
        }
        
        int rand = Random.Range(0, items.Count);
        Item itemToHolofoil = items[rand];
        if(itemToHolofoil != null)
        {
            itemToHolofoil.SetHolofoil();
            holofoilVFX.Spawn(itemToHolofoil.transform.position);
            holofoilSFX.Play();
        }
        winningSFX.Play();

        yield return new WaitForSeconds(1f);
    }

    IEnumerator GiveRandomItemTask(DialogueContext dc)
    {
        List<Item> items = randomItemCollection.GetItemsByRarity(Rarity.Rare);
        int rand = Random.Range(0, items.Count);
        Item itemPrefab = items[rand];

        if (itemPrefab != null)
        {
            ItemSlot itemSlot = GameObject.Instantiate(Singleton.Instance.itemManager.itemSlotPrefab);
            itemSlot.transform.parent = dc.dialogueBox.itemSlotParent;
            itemSlot.transform.localPosition = new Vector3(0f,0f,0f);
            itemSlot.isEventSlot = true;
            
            Item item = Singleton.Instance.itemManager.GenerateItemWithWrapper(itemPrefab);
            Singleton.Instance.itemManager.AddItemToSlot(item, itemSlot);
            
            Task itemMadeTask = new Task(randomItemLine.RunTask(dc));
            while (itemMadeTask.Running)
            {
                yield return null;
            }
            
            if (itemSlot.currentItem != null)
            {
                Singleton.Instance.itemManager.MoveItemToEmptyInventorySlot(item, 0.25f);
                yield return new WaitForSeconds(0.5f);
            }
            
            Destroy(itemSlot.gameObject);
        }
    }

    IEnumerator AddLifeTask()
    {
        Singleton.Instance.playerStats.AddLife(1);
        yield return new WaitForSeconds(1f);
    }
}
