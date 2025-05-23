using System;
using UnityEngine;

public class MakeSickOnBallCollide : MonoBehaviour
{
    public PooledObjectData collideVFX;
    public SFXInfo collideSFX;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball b = other.gameObject.GetComponent<Ball>();
        if (b == null)
        {
            return;
        }
        Singleton.Instance.survivalManager.MakeSick();
        collideVFX.Spawn(this.transform.position);
        collideSFX.Play();
        gameObject.SetActive(false);
    }
}
