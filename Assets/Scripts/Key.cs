using System;
using UnityEngine;

public class Key : MonoBehaviour
{
    public PooledObjectData collectVFX;
    public SFXInfo collectSFX;
    public static Action KeyCollectedEvent;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball b = other.gameObject.GetComponent<Ball>();
        
        if (b != null)
        {
            CollectKey();
            return;
        }
        
        Bonker bonker = other.gameObject.GetComponent<Bonker>();
        if (bonker != null && bonker.treatAsBall)
        {
            CollectKey();
            return;
        }
    }

    void CollectKey()
    {
        collectVFX.Spawn(this.transform.position);
        collectSFX.Play(this.transform.position);
        Singleton.Instance.playerStats.AddKey(1);
        KeyCollectedEvent?.Invoke();
        gameObject.SetActive(false);
    }
}
