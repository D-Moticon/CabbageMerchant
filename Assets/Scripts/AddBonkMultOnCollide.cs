using System;
using UnityEngine;

public class AddBonkMultOnCollide : MonoBehaviour
{
    public float baseBonkMultAdd = 0.5f;
    float bonkMultAdd = 0.5f;
    public int bouncesBeforeDestroy = 0;
    private int bounceCounter;
    public PooledObjectData bonkMultVFX;
    public SFXInfo bonkMultSFX;
    public Material baseMat;
    public Material rainbowMat;
    
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            c.AddBonkMultiplier(bonkMultAdd);
            c.PlayBonkFX();
            
            if (bonkMultVFX != null)
            {
                bonkMultVFX.Spawn(other.GetContact(0).point);
            }
            bonkMultSFX.Play();
            
            bounceCounter++;
        }

        if (bounceCounter > bouncesBeforeDestroy)
        {
            
            gameObject.SetActive(false);
        }
    }
    
    public void SetRainbow()
    {
        if (rainbowMat != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            sr.material = rainbowMat;
        }
        
        bonkMultAdd = baseBonkMultAdd*2f;
    }
    
    public void SetNormal()
    {
        if (baseMat != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            sr.material = baseMat;
        }

        bonkMultAdd = baseBonkMultAdd;
    }
}
