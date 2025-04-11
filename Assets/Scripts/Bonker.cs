using System;
using UnityEngine;

public class Bonker : MonoBehaviour
{
    public float bonkValue = 1f;
    public bool deactivateOnBonk;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            c.Bonk(bonkValue, other.GetContact(0).point);

            if (deactivateOnBonk)
            {
                gameObject.SetActive(false);
            }
            
        }
    }
}
