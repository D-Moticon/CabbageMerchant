using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using TMPro;

public class MultiplyBonkMultOnCollide : MonoBehaviour
{
    public float bonkMultMult = 1.1f;
    public PooledObjectData vfx;
    public SFXInfo sfx;
    public MMF_Player feelPlayer;
    public TMP_Text multText;

    private void OnEnable()
    {
        if (multText != null)
        {
            multText.text = $"<size=17>x</size>{bonkMultMult:F2}";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CollisionOccured(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CollisionOccured(other.collider);
    }

    void CollisionOccured(Collider2D other)
    {
        Cabbage c = other.GetComponent<Cabbage>();
        if (c == null)
        {
            return;
        }
        
        c.MultiplyBonkMultiplier(bonkMultMult);
        if (vfx != null)
        {
            vfx.Spawn(c.transform.position);
        }

        if (feelPlayer != null)
        {
            feelPlayer.PlayFeedbacks();
        }

        sfx.Play();
    }
}
