using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Bonker : MonoBehaviour
{
    public float bonkValue = 1f;
    public bool killOnBonk;
    public int bonksBeforeKill = 0;
    private int bonkCounter = 0;
    public bool treatAsBall = false;

    private void OnEnable()
    {
        bonkCounter = 0;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            c.Bonk(bonkValue, other.GetContact(0).point, other.GetContact(0).normal, null, treatAsBall);

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        Cabbage c = other.gameObject.GetComponent<Cabbage>();
        if (c != null)
        {
            Vector2 pos = (this.transform.position + other.transform.position) / 2f;
            Vector2 normal = (this.transform.position - other.transform.position).normalized;
            c.Bonk(bonkValue, pos, normal, null, treatAsBall);

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
