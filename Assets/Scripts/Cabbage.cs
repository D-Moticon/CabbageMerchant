using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Cabbage : MonoBehaviour
{
    public Rigidbody2D rb;
    public PooledObjectData bonkVFX;
    public PooledObjectData popVFX;
    public MMF_Player bonkFeel;
    public Color floaterColor = Color.white;
    public SFXInfo bonkSFX;
    public SFXInfo popSFX;
    [HideInInspector]public int sizeLevel = 0;
    [HideInInspector]public int colorLevel = 0;
    public float huePerLevel = 0.05f;
    public float maxHue = 0.65f;
    public float startingScale = 0.5f;
    public float scalePerLevel = 0.2f;
    private SpriteRenderer sr;
    public bool isMerging = false;

    void Start()
    {
        rb.bodyType = RigidbodyType2D.Static;
        sr = GetComponentInChildren<SpriteRenderer>();

        float sca = startingScale + sizeLevel * scalePerLevel;
        transform.localScale = new Vector3(sca, sca, 1f);
        UpdateSizeLevel();
        UpdateColorLevel();
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
            Cabbage c = col.GetComponent<Cabbage>();
            
            if (col.gameObject != this.gameObject && c != null)
            {
                if (c.enabled)
                {
                    Merge(c);
                }
                
            }
        }
    }

    public void Bonk(Vector2 collisionPos)
    {
        sizeLevel++;

        UpdateSizeLevel();

        bonkSFX.Play();
        bonkVFX.Spawn(collisionPos, Quaternion.identity);
        bonkFeel.PlayFeedbacks(this.transform.position,1f);
    }

    public void Pop(Vector2 collisionPos)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        popVFX.Spawn(collisionPos, Quaternion.identity);
        bonkFeel.PlayFeedbacks(this.transform.position,2f,false);
        GameSingleton.Instance.screenShaker.ShakeScreen();
        Singleton.Instance.floaterManager.SpawnFloater("100", this.transform.position, floaterColor);
        rb.angularVelocity = Random.Range(-400f, 400f);
        popSFX.Play();
        gameObject.layer = LayerMask.NameToLayer("TransparentFX");
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

    }

    private void OnCollisionStay(Collision other)
    {

    }

    void Merge(Cabbage otherCabbage)
    {
        if (isMerging || otherCabbage.isMerging)
        {
            return;
        }

        isMerging = true;
        otherCabbage.isMerging = true;

        Vector2 pos = (this.transform.position + otherCabbage.transform.position) * 0.5f;
        int newColorLevel = Mathf.Max(this.colorLevel, otherCabbage.colorLevel) + 1;
        int newSizeLevel = Mathf.Max(this.sizeLevel, otherCabbage.sizeLevel) + 1;

        Cabbage c = GameSingleton.Instance.gameStateMachine.cabbagePooledObject.Spawn(pos, Quaternion.identity)
            .GetComponent<Cabbage>();
        c.colorLevel = newColorLevel;
        c.sizeLevel = newSizeLevel;
        c.UpdateColorLevel();
        c.UpdateSizeLevel();
        c.bonkFeel.PlayFeedbacks(this.transform.position,2f);
        c.popSFX.Play();
        GameObject pVFX = c.popVFX.Spawn(pos, Quaternion.identity);
        float sca = sizeLevel * scalePerLevel;
        pVFX.transform.localScale = new Vector3(sca, sca, 1f);
        

        this.gameObject.SetActive(false);
        otherCabbage.gameObject.SetActive(false);
    }

    public void UpdateColorLevel()
    {
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(mpb);
        float newHue = Mathf.Min(colorLevel * huePerLevel, maxHue);
        mpb.SetFloat("_Hue", newHue);
        sr.SetPropertyBlock(mpb);
    }

    public void UpdateSizeLevel()
    {
        float sca = startingScale + sizeLevel * scalePerLevel;
        transform.localScale = new Vector3(sca, sca, 1f);
    }
}