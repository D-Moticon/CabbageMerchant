using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections.Generic;

public class Cabbage : MonoBehaviour
{
    public enum GrowthMode
    {
        Linear,
        Logarithmic,
        Root
    }

    [Header("References")]
    public Rigidbody2D rb;
    public PooledObjectData bonkVFX;
    public PooledObjectData popVFX;
    public MMF_Player bonkFeel;
    public MMF_Player popFeel;
    public Color floaterColor = Color.white;
    public SFXInfo bonkSFX;
    public SFXInfo popSFX;
    public TMP_Text pointsText;
    public MMF_Player pointsTextFeel;
    public FloaterReference popFloater;
    public FloaterReference scoreFloater;

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
    private SpriteRenderer sr;

    [Header("Scoring")]
    public float startingPoints = 0f;

    public double pointsPerSize = 1;
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

    void Start()
    {
        rb.bodyType = RigidbodyType2D.Static;
        sr = GetComponentInChildren<SpriteRenderer>();

        UpdateSizeLevel();
        UpdateColorLevel();
        UpdatePoints();
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

    public void Bonk(float bonkValue, Vector2 collisionPos, Vector2 normal = default)
    {
        sizeLevel = Mathf.Min(sizeLevel + bonkValue, maxSizeLevel);
        UpdateSizeLevel();

        // VFX and feedback
        bonkSFX.Play(collisionPos, bonkSFX.vol * bonkValue);
        GameObject vfx = bonkVFX.Spawn(collisionPos, Quaternion.identity);
        vfx.transform.localScale = new Vector3(bonkValue, bonkValue, 1f);

        float sca = transform.localScale.x;
        float intensity = 1f / sca;
        bonkFeel.PlayFeedbacks(transform.position, intensity);
        pointsTextFeel.PlayFeedbacks(transform.position, intensity);
        
        UpdatePoints();
    }

    public void Pop(Vector2 collisionPos)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        popVFX.Spawn(collisionPos, Quaternion.identity);
        bonkFeel.PlayFeedbacks(transform.position, 2f, false);
        Singleton.Instance.screenShaker.ShakeScreen();
        Singleton.Instance.floaterManager.SpawnFloater(popFloater, "100", transform.position, floaterColor);

        rb.angularVelocity = Random.Range(-400f, 400f);
        popSFX.Play(collisionPos);
        gameObject.layer = LayerMask.NameToLayer("TransparentFX");
    }

    private void OnCollisionEnter2D(Collision2D other) { }
    private void OnCollisionStay(Collision other) { }

    void UpdatePoints()
    {
        points = sizeLevel * pointsPerSize * (1 + pointMultPerColor * colorLevel);
        pointsText.text = Helpers.FormatWithSuffix(points);
    }
    
    void Merge(Cabbage otherCabbage)
    {
        if (isMerging || otherCabbage.isMerging) return;

        isMerging = true;
        otherCabbage.isMerging = true;

        Vector2 pos = 0.5f * (transform.position + otherCabbage.transform.position);
        int newColorLevel = Mathf.Min(Mathf.Max(colorLevel, otherCabbage.colorLevel) + 1, maxColorLevel);
        float newSizeLevel = Mathf.Min(Mathf.Max(sizeLevel, otherCabbage.sizeLevel) + 1, maxSizeLevel);

        Cabbage c = GameSingleton.Instance.gameStateMachine
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
        
        gameObject.SetActive(false);
        otherCabbage.gameObject.SetActive(false);
        
        
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
}
