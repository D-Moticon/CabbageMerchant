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
    public PooledObjectData pooledObjectReference;
    public Rigidbody2D rb;
    public Collider2D col;
    public bool unPoppable = false;
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
    public float sizeLevel;
    [HideInInspector] public int colorLevel;
    public int maxSizeLevel = 1000;
    public int maxColorLevel = 100;
    public float huePerLevel = 0.05f;
    public float maxHue = 0.65f;
    public double startingPoints = 0;
    public int startingSizeLevel = 0;

    [Header("Scale Controls")]
    public float startingScale = 0.5f;
    public float scalePerLevel = 0.2f;

    public bool reverseGrowth = false;
    
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
    [HideInInspector] public bool isDynamic = false;

    [Header("Scoring")]
    public double baseBonkMultiplier = 1;
    [HideInInspector]public double bonkMultiplier = 1;
    [HideInInspector]public double points = 0;
    public float bonkMultPerColor = 1.5f;
    public TMP_Text pointMultText;

    [Header("Merging")]
    public bool forceThisTypeOnMerge = false;

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
        [FormerlySerializedAs("cabbageVariantPrefab")] public PooledObjectData cabbageVariantPooledObject;
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
    public static event CabbageMergedDelegate CabbageMergedEventPreDestroy;

    public class CabbagePoppedParams
    {
        public Cabbage c;
    }

    public delegate void CabbagePoppedDelegate(CabbagePoppedParams cpp);
    public static event CabbagePoppedDelegate CabbagePoppedEvent;

    public delegate void CabbageEvent(Cabbage c);
    public static CabbageEvent CabbageSpawnedEvent;
    public static CabbageEvent CabbageRemovedEvent;
    
    bool isHarvesting = false;
    
    private void OnEnable()
    {
        spawnSFX.Play();
        points = startingPoints;
        sizeLevel = startingSizeLevel;
        bonkMultiplier = baseBonkMultiplier;
        isMerging   = false;
        isHarvesting = false;
        UpdateSizeLevel();
        rb.bodyType = RigidbodyType2D.Kinematic;
        isDynamic = false;
        
        CabbageSpawnedEvent?.Invoke(this);
    }

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        UpdateSizeLevel();
        UpdateColorLevel();
        UpdateBonkValueDisplay();
        wallLayerMask = LayerMask.NameToLayer("Wall");
    }

    void Update()
    {
        UpdateBonkValueDisplay();
        CheckForOverlaps();
    }

    void CheckForOverlaps()
    {
        if (isHarvesting)
        {
            return;
        }
        
        float rad = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, rad);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == this.gameObject) continue;

            Cabbage c = col.GetComponent<Cabbage>();
            if (c != null && c.gameObject.activeInHierarchy && c.enabled)
            {
                Merge(c);
            }
        }
    }

    public void Bonk(BonkParams bp)
    {
        if (reverseGrowth)
        {
            bp.bonkerPower *= -1;
        }
        
        sizeLevel = Mathf.Min(sizeLevel + (float)bp.bonkerPower, maxSizeLevel);
        if (sizeLevel < 0)
        {
            sizeLevel = 0;
        }
        UpdateSizeLevel();

        // VFX and feedback
        if (!bp.overrideSFX)
        {
            float vol = Mathf.Max(0.35f, (float)Math.Abs(bp.bonkerPower));
            bonkSFX.Play(bp.collisionPos, bonkSFX.vol * vol);
        }

        GameObject vfx = bonkVFX.Spawn(bp.collisionPos, Quaternion.identity);
        vfx.transform.localScale = new Vector3(1f+(float)bp.bonkerPower*0.1f, 1f+(float)bp.bonkerPower*0.1f, 1f);

        PlayBonkFX();

        points += (bp.bonkerPower * bonkMultiplier);
        if (points < 0)
        {
            points = 0;
        }
        
        bp.bonkedCabbage = this;
        bp.bonkable = this;
        bp.totalBonkValueGained = bp.bonkerPower * bonkMultiplier;

        CabbageBonkedEvent?.Invoke(bp);

        if (currentVariant != null)
        {
            currentVariant.CabbageBonked(bp);
        }
    }

    public void PlayBonkFX()
    {
        float sca = transform.localScale.x;
        float intensity = 1f / sca;
        bonkFeel.PlayFeedbacks(transform.position, intensity);
        pointsTextFeel.PlayFeedbacks(transform.position, intensity);
    }

    public void Remove()
    {
        CabbageRemovedEvent?.Invoke(this);
        
        if (GameSingleton.Instance != null)
        {
            GameSingleton.Instance.gameStateMachine.RemoveActiveCabbage(this);
            GameSingleton.Instance.objectPoolManager.ReturnToPool(pooledObjectReference, this.gameObject);
        }
        
        gameObject.SetActive(false);
    }

    public void FullReset()
    {
        points = 0;
        sizeLevel = startingSizeLevel;
        bonkMultiplier = baseBonkMultiplier;
        colorLevel = 0;
        UpdateSizeLevel();
        UpdateColorLevel();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Pop(Vector2 collisionPos)
    {
        CabbagePoppedParams cpp = new CabbagePoppedParams();
        cpp.c = this;
        CabbagePoppedEvent?.Invoke(cpp);
        
        Color col = Color.white;
        PlayPopVFX();
        popSFX.Play();
        gameObject.SetActive(false);

        if (GameSingleton.Instance != null)
        {
            GameSingleton.Instance.gameStateMachine.BankPoints(points);
        }
    }

    public void Harvest()
    {
        //OLD
        rb.bodyType = RigidbodyType2D.Dynamic;
        bonkFeel.PlayFeedbacks(transform.position, 2f, false);
        rb.angularVelocity = Random.Range(-400f, 400f);
        gameObject.layer = LayerMask.NameToLayer("Water");
        isHarvesting = true;
    }

    public void MakeDynamic()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        isDynamic = true;
    }

    public void MakeUnDynamic()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isDynamic = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == wallLayerMask
            && other.gameObject.GetComponent<Vine>() == null
            && GameSingleton.Instance.gameStateMachine.GetNumberActiveCabbages() > 1
            && !isDynamic
            && !unPoppable)
        {
            Pop(other.GetContact(0).point);
        }

        if (isDynamic)
        {
            Cabbage c = col.GetComponent<Cabbage>();
            if (c != null && c.enabled)
            {
                BonkParams bp = new BonkParams();
                bp.bonkedCabbage = c;
                bp.bonkerPower = Singleton.Instance.playerStats.dynamicCabbageBonkPower;
                bp.collisionPos = other.GetContact(0).point;
                c.Bonk(bp);
            }
        }
    }

    void UpdateBonkValueDisplay()
    {
        pointsText.text = Helpers.FormatWithSuffix(points);
        if (pointMultText != null)
        {
            if (bonkMultiplier > 1.1)
            {
                pointMultText.text = $"<size=17>x</size>{Helpers.FormatWithSuffix(bonkMultiplier)}";
            }
            else
            {
                pointMultText.text = "";
            }
        }
    }
    
    void Merge(Cabbage otherCabbage)
    {
        if (isHarvesting || otherCabbage.isHarvesting) return;   
        if (isMerging || otherCabbage.isMerging) return;
        if (!col.enabled || !otherCabbage.col.enabled) return;
        if (isDynamic) return;
        if (unPoppable) return;
        
        isMerging = true;
        otherCabbage.isMerging = true;

        Vector2 pos = 0.5f * (transform.position + otherCabbage.transform.position);
        int newColorLevel = Mathf.Min(Mathf.Max(colorLevel, otherCabbage.colorLevel) + 1, maxColorLevel);
        float newSizeLevel = Mathf.Min(Mathf.Max(sizeLevel, otherCabbage.sizeLevel) + 1, maxSizeLevel);

        PooledObjectData objToSpawn = pooledObjectReference;
        if (otherCabbage.forceThisTypeOnMerge)
        {
            objToSpawn = otherCabbage.pooledObjectReference;
        }
        
        Cabbage c = objToSpawn.Spawn(pos, Quaternion.identity)
            .GetComponent<Cabbage>();

        c.colorLevel = newColorLevel;
        c.points = otherCabbage.points + points;
        c.bonkMultiplier = (otherCabbage.bonkMultiplier+bonkMultiplier) * bonkMultPerColor;
        c.sizeLevel = newSizeLevel;
        c.UpdateColorLevel();
        c.UpdateSizeLevel();

        c.bonkFeel.PlayFeedbacks(transform.position, 2f);
        c.popSFX.Play(pos);
        c.popFeel.PlayFeedbacks();
        c.pointsTextFeel.PlayFeedbacks();
        c.UpdateBonkValueDisplay();
        
        GameSingleton.Instance.gameStateMachine.AddActiveCabbage(c);

        // Spawn VFX at the merged position
        GameObject pVFX = c.popVFX.Spawn(pos, Quaternion.identity);
        float sca = scalePerLevel * Mathf.Pow(sizeLevel, rootExponent);
        sca = Mathf.Min(sca, 4f);
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
        
        CabbageMergedParams cmp = new CabbageMergedParams();
        cmp.pos = pos;
        cmp.newCabbage = c;
        cmp.scale = sca;
        cmp.oldCabbageA = this;
        cmp.oldCabbageB = otherCabbage;
        
        CabbageMergedEventPreDestroy?.Invoke(cmp);
        
        gameObject.SetActive(false);
        otherCabbage.gameObject.SetActive(false);
        
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
        sca = Mathf.Min(sca, 4f);
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
        PooledObjectData variantPooledObject = null;

        for (int i = 0; i < variantInfos.Count; i++)
        {
            if (variantInfos[i].variantType == cvt)
            {
                variantPooledObject = variantInfos[i].cabbageVariantPooledObject;
            }
        }
        
        if (variantPooledObject == null)
        {
            print($"No prefab found for {cvt})");
            return;
        }
        
        if (currentVariant != null)
        {
            currentVariant.RemoveVariant();
            currentVariant = null;
        }
        
        CabbageVariant variant = variantPooledObject.Spawn(this.transform.position).GetComponent<CabbageVariant>();
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
        //points = 0;
        //bonkMultiplier = 1;
    }

    public void AddBonkMultiplier(double multAdd)
    {
        bonkMultiplier += multAdd;
    }

    public void MultiplyBonkMultiplier(double multMult)
    {
        bonkMultiplier *= multMult;
        Singleton.Instance.floaterManager.SpawnInfoFloater($"x{multMult:F1}", this.transform.position + new Vector3(0f,.3f,0f), Color.green, 0.75f);
    }
}
