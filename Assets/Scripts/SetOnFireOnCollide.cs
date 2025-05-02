using System;
using UnityEngine;

public class SetOnFireOnCollide : MonoBehaviour
{
    public int stacksToAdd = 3;
    public PooledObjectData vfx;
    public SFXInfo sfx;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        SetOnFire(other.collider, other.GetContact(0).point);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SetOnFire(other, this.transform.position);
    }

    void SetOnFire(Collider2D other, Vector2 pos)
    {
        IBonkable bonkable = other.GetComponent<IBonkable>();
        if (bonkable == null)
        {
            vfx.Spawn(pos);
            sfx.Play(pos, 0.2f);
            this.gameObject.SetActive(false);
            return;
        }

        Fire.SetBonkableOnFire(bonkable, stacksToAdd);

        if (vfx != null)
        {
            vfx.Spawn(pos);
        }

        sfx.Play();
        
        this.gameObject.SetActive(false);
    }
}
