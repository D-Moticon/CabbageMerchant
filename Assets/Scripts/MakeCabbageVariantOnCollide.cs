using System;
using UnityEngine;

public class MakeCabbageVariantOnCollide : MonoBehaviour
{
    public CabbageVariantType cabbageVariantType;
    public float chance = 1f;

    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c == null)
        {
            return;
        }

        else
        {
            ConvertCabbage(c);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c == null)
        {
            return;
        }

        else
        {
            ConvertCabbage(c);
        }
    }

    void ConvertCabbage(Cabbage c)
    {
        float rand = UnityEngine.Random.Range(0f, 1f);
        if (rand <= chance)
        {
            c.SetVariant(cabbageVariantType);
        }
        
    }
}
