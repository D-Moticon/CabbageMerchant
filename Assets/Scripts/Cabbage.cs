using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class Cabbage : MonoBehaviour, IBonkable
{
    public enum GrowthMode
    {
        Linear,
        Logarithmic,
        Root,
        None
    }

    [Header("References")]
    public Rigidbody2D rb;
    public Collider2D col;
    public SpriteRenderer sr;
    public PooledObjectData bonkVFX;
    public PooledObjectData popVFX;
    public MMF_Player bonkFeel;
    public MMF_Player popFeel;
    public Color floaterColor = Color.white;
    public SFXInfo bonkSFX;
    public SFXInfo popSFX;
    public SFXInfo spawnSFX;
    public TMP_Text pointsText;
    public MMF_Player pointsTextFeel;
    public FloaterReference popFloater;
    public FloaterReference scoreFloater;
    public Material baseMaterial;

    [Header("Levels & Hue")]
    [HideInInspector] public float sizeLevel;
    [HideInInspector] public int colorLevel;
    public int maxSizeLevel = 1000;
    public int maxColorLevel = 100;
    public float huePerLevel = 0.05f;
    public float maxHue = 0.65f;

    [Header("Scale Controls")]
    public float startingScale = 0.5f;
    public float scalePerLevel = 0.2f;

    [FoldoutGroup("Advanced Growth")]
    [EnumToggleButtons]
    public GrowthMode growthMode = GrowthMode.Root;

    [FoldoutGroup("Advanced Growth"), ShowIf("@growthMode == GrowthMode.Logarithmic")]
    [LabelText("Log Base")]
    public float logBase = 1.2f;

    [FoldoutGroup("Advanced Growth"), ShowIf("@growthMode == GrowthMode.Logarithmic")]
    [LabelText("Log Multiplier")]
    public float logMultiplier = 1f;

    [FoldoutGroup("Advanced Growth"), ShowIf("@growthMode == GrowthMode.Root")]
    [LabelText("Root Exponent")]
    [Tooltip("Typical range: 0.5-1.0. Lower = gentler growth.")]
    public float rootExponent = 0.7f;

    [HideInInspector] public bool isMerging = false;
    [HideInInspector] public bool isStolen = false;

    [Header("Scoring")]
    public float startingPoints = 0f;

    public double basePointsPerSize = 1;
    [HideInInspector]public double currentPointsPerSize = 1;
    public float pointMultPerColor = 1.5f;
    [HideInInspector]public double points;

    [System.Serializable]
    public class ScoringInfo
    {
        public int colorThreshold;
        public SFXInfo sfx;
        public float screenShakeIntensity = 0f;
    }

    public List<ScoringInfo> scoringInfos;

    public delegate void CabbageBonkedDelegate(BonkParams bp);
    public static event CabbageBonkedDelegate CabbageBonkedEvent;

    private LayerMask wallLayerMask;
    
    [System.Serializable]
    public class VariantInfo
    {
        public CabbageVariantType variantType;
        public CabbageVariant cabbageVariantPrefab;
    }

    public List<VariantInfo> variantInfos;
    public CabbageVariant currentVariant;
    public CabbageVariantType currentVariantType;

    public class CabbageMergedParams
    {
        public Cabbage newCabbage;
        public Cabbage oldCabbageA;
        public Cabbage oldCabbageB;
        public Vector2 pos;
        public float scale;
    }

    public delegate void CabbageMergedDelegate(CabbageMergedParams cpp);

    public static event CabbageMergedDelegate CabbageMergedEvent;
    
    private void OnEnable()
    {
        spawnSFX.Play();
    }

    void Start()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        sr = GetComponentInChildren<SpriteRenderer>();

        UpdateSizeLevel();
        UpdateColorLevel();
        UpdatePoints();
        wallLayerMask = LayerMask.NameToLayer("Wall");
    }

    void Update()
    {
        CheckForOverlaps();
    }

    void CheckForOverlaps()
    {
        float rad = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, rad);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == this.gameObject) continue;

            Cabbage c = col.GetComponent<Cabbage>();
            if (c != null && c.enabled)
            {
                Merge(c);
            }
        }
    }

    public void Bonk(BonkParams bp)
    {
        sizeLevel = Mathf.Min(sizeLevel + bp.bonkValue, maxSizeLevel);
        UpdateSizeLevel();

        // VFX and feedback
        bonkSFX.Play(bp.collisionPos, bonkSFX.vol * bp.bonkValue);
        GameObject vfx = bonkVFX.Spawn(bp.collisionPos, Quaternion.identity);
        vfx.transform.localScale = new Vector3(bp.bonkValue, bp.bonkValue, 1f);

        float sca = transform.localScale.x;
        float intensity = 1f / sca;
        bonkFeel.PlayFeedbacks(transform.position, intensity);
        pointsTextFeel.PlayFeedbacks(transform.position, intensity);
        
        UpdatePoints();
        
        bp.bonkedCabbage = this;
        bp.bonkable = this;

        CabbageBonkedEvent?.Invoke(bp);

        if (currentVariant != null)
        {
            currentVariant.CabbageBonked(bp);
        }
    }

    public void Pop(Vector2 collisionPos)
    {
        Color col = Color.white;
        PlayPopVFX();
        popSFX.Play();
        gameObject.SetActive(false);

        //OLD
        /*rb.bodyType = RigidbodyType2D.Dynamic;
        popVFX.Spawn(collisionPos, Quaternion.identity);
        bonkFeel.PlayFeedbacks(transform.position, 2f, false);
        Singleton.Instance.screenShaker.ShakeScreen();
        Singleton.Instance.floaterManager.SpawnFloater(popFloater, "100", transform.position, floaterColor);

        rb.angularVelocity = Random.Range(-400f, 400f);
        popSFX.Play(collisionPos);
        gameObject.layer = LayerMask.NameToLayer("TransparentFX");*/


    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == wallLayerMask)
        {
            Pop(other.GetContact(0).point);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        
    }

    void UpdatePoints()
    {
        points = sizeLevel * currentPointsPerSize * (1 + pointMultPerColor * colorLevel);
        pointsText.text = Helpers.FormatWithSuffix(points);
    }
    
    void Merge(Cabbage otherCabbage)
    {
        if (isMerging || otherCabbage.isMerging) return;
        if (!col.enabled || !otherCabbage.col.enabled)
        {
            return;
        }
        
        isMerging = true;
        otherCabbage.isMerging = true;

        Vector2 pos = 0.5f * (transform.position + otherCabbage.transform.position);
        int newColorLevel = Mathf.Min(Mathf.Max(colorLevel, otherCabbage.colorLevel) + 1, maxColorLevel);
        float newSizeLevel = Mathf.Min(Mathf.Max(sizeLevel, otherCabbage.sizeLevel) + 1, maxSizeLevel);

        Cabbage c = GameSingleton.Instance.currentBiomeParent
            .cabbagePooledObject.Spawn(pos, Quaternion.identity)
            .GetComponent<Cabbage>();

        c.colorLevel = newColorLevel;
        c.sizeLevel = newSizeLevel;
        c.UpdateColorLevel();
        c.UpdateSizeLevel();

        c.bonkFeel.PlayFeedbacks(transform.position, 2f);
        c.popSFX.Play(pos);
        c.popFeel.PlayFeedbacks();
        c.pointsTextFeel.PlayFeedbacks();
        c.UpdatePoints();
        
        GameSingleton.Instance.gameStateMachine.AddActiveCabbage(c);

        // Spawn VFX at the merged position
        GameObject pVFX = c.popVFX.Spawn(pos, Quaternion.identity);
        float sca = scalePerLevel * Mathf.Pow(sizeLevel, rootExponent);
        pVFX.transform.localScale = new Vector3(sca, sca, 1f);
        Singleton.Instance.screenShaker.ShakeScreen();

        GameSingleton.Instance.gameStateMachine.RemoveActiveCabbage(this);
        GameSingleton.Instance.gameStateMachine.RemoveActiveCabbage(otherCabbage);

        if (this.currentVariantType != CabbageVariantType.none)
        {
            c.SetVariant(this.currentVariantType);
        }
        
        else if (otherCabbage.currentVariantType != CabbageVariantType.none)
        {
            c.SetVariant(otherCabbage.currentVariantType);
        }
        
        SetNoVariant();
        otherCabbage.SetNoVariant();
        gameObject.SetActive(false);
        otherCabbage.gameObject.SetActive(false);

        CabbageMergedParams cmp = new CabbageMergedParams();
        cmp.pos = pos;
        cmp.newCabbage = c;
        cmp.scale = sca;
        cmp.oldCabbageA = this;
        cmp.oldCabbageB = otherCabbage;
        CabbageMergedEvent?.Invoke(cmp);
    }

    public void UpdateColorLevel()
    {
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(mpb);

        float newHue = Mathf.Min(colorLevel * huePerLevel, maxHue);
        mpb.SetFloat("_Hue", newHue);

        sr.SetPropertyBlock(mpb);
    }

    public void UpdateSizeLevel()
    {
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();

        float sca;
        switch (growthMode)
        {
            case GrowthMode.Linear:
                // Straight line growth
                sca = startingScale + sizeLevel * scalePerLevel;
                break;

            case GrowthMode.Logarithmic:
                // Slower at higher levels
                sca = startingScale + scalePerLevel * logMultiplier * Mathf.Log(sizeLevel + 1, logBase);
                break;

            case GrowthMode.Root:
                // Gentle, unbounded growth (sizeLevel^(rootExponent < 1) grows slower than linear but faster than log for large numbers).
                sca = startingScale + scalePerLevel * Mathf.Pow(sizeLevel, rootExponent);
                break;

            case GrowthMode.None:
                sca = transform.localScale.x;
                break;
            
            default:
                sca = startingScale;
                break;
        }

        transform.localScale = new Vector3(sca, sca, 1f);
    }

    public void PlayPopVFX()
    {
        GameObject pVFX = popVFX.Spawn(transform.position, Quaternion.identity);
        float sca = scalePerLevel * Mathf.Pow(sizeLevel, rootExponent);
        pVFX.transform.localScale = new Vector3(sca, sca, 1f);
    }

    public void PlayScoringSFX()
    {
        for (int i = scoringInfos.Count - 1; i >= 0; i--)
        {
            if (colorLevel >= scoringInfos[i].colorThreshold)
            {
                scoringInfos[i].sfx.Play();
                Singleton.Instance.screenShaker.ShakeScreen(scoringInfos[i].screenShakeIntensity);
                break;
            }
        }
    }

    public void SetVariant(CabbageVariantType cvt)
    {
        CabbageVariant variantPrefab = null;

        for (int i = 0; i < variantInfos.Count; i++)
        {
            if (variantInfos[i].variantType == cvt)
            {
                variantPrefab = variantInfos[i].cabbageVariantPrefab;
            }
        }
        
        if (variantPrefab == null)
        {
            print($"No prefab found for {cvt})");
            return;
        }
        
        if (currentVariant != null)
        {
            currentVariant.RemoveVariant();
            currentVariant = null;
        }

        CabbageVariant variant = Instantiate(variantPrefab, transform);
        variant.transform.localPosition = Vector3.zero;
        currentVariant = variant;
        currentVariantType = cvt;
        
        variant.Initialize(this);
    }

    public void SetNoVariant()
    {
        if (currentVariant != null)
        {
            currentVariant.RemoveVariant();
            currentVariant = null;
        }

        currentVariantType = CabbageVariantType.none;
        sr.material = baseMaterial;
        currentPointsPerSize = basePointsPerSize;
    }

    public void AddPointsPerSize(double pointAdd)
    {
        currentPointsPerSize += pointAdd;
    }
}
