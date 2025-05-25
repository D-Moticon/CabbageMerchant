using System;
using UnityEngine;

public class SetOnFireOnCollide : MonoBehaviour
{
    public Rigidbody2D rb;
    public Collider2D col;
    public SpriteRenderer sr;
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

    public void Despawn()
    {
        // 1) hide visuals
        sr.enabled = false;

        // 2) stop physics & disable collisions
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;         // stops physics simulation
        //col.enabled = false;   // turns off collision callbacks

        // 3) (optional) move it out of the way
        transform.position = Singleton.Instance.offScreenPosition;
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
        
        //Rather than setting inactive, using this custom Despawn method.
        //Activating and deactivating caused huge lag spikes with many balls.  This is much better
        //this.gameObject.SetActive(false);
        Despawn();
    }
}
