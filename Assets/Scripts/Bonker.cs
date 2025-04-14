using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Bonker : MonoBehaviour
{
    public float bonkValue = 1f;
    public bool killOnBonk;
    public int bonksBeforeKill = 0;
    private int bonkCounter = 0;

    private void OnEnable()
    {
        bonkCounter = 0;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            c.Bonk(bonkValue, other.GetContact(0).point);

            if (killOnBonk)
            {
                if (bonkCounter >= bonksBeforeKill)
                {
                    gameObject.SetActive(false);
                }

                bonkCounter++;
            }
            
        }
    }
}
