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
        IBonkable b = other.gameObject.GetComponent<IBonkable>();
        if (b != null)
        {
            BonkParams bp = new BonkParams();
            bp.bonkValue = bonkValue;
            bp.collisionPos = other.GetContact(0).point;
            bp.normal = other.GetContact(0).normal;
            bp.ball = null;
            bp.treatAsBall = treatAsBall;
            b.Bonk(bp);

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
        IBonkable b = other.gameObject.GetComponent<IBonkable>();
        if (b != null)
        {
            Vector2 pos = (this.transform.position + other.transform.position) / 2f;
            Vector2 normal = (this.transform.position - other.transform.position).normalized;
            
            BonkParams bp = new BonkParams();
            bp.bonkValue = bonkValue;
            bp.collisionPos = pos;
            bp.normal = normal;
            bp.ball = null;
            bp.treatAsBall = treatAsBall;
            b.Bonk(bp);

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
