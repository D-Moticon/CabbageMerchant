using System;
using UnityEngine;
using MoreMountains.Feedbacks;

public class Enemy : MonoBehaviour, IBonkable
{
    public float maxHP = 10f;
    [HideInInspector] public float hp;
    public MMF_Player bonkFeel;
    public PooledObjectData bonkVFX;
    public SFXInfo bonkSFX;
    public PooledObjectData killVFX;
    public SFXInfo killSFX;

    private void OnEnable()
    {
        hp = maxHP;
    }

    private void OnDisable()
    {
        //throw new NotImplementedException();
    }

    public void Bonk(BonkParams bp)
    {
        hp -= bp.bonkerPower;
        if (bonkVFX != null)
        {
            bonkVFX.Spawn(bp.collisionPos);
        }
        bonkFeel.PlayFeedbacks();
        bonkSFX.Play(this.transform.position);
        if (hp <= 0f)
        {
            KillEnemy();
        }
    }

    public void Remove()
    {
        gameObject.SetActive(false);
    }

    void KillEnemy()
    {
        if (killVFX != null)
        {
            killVFX.Spawn(this.transform.position);
        }

        IKillable[] killables = GetComponentsInChildren<IKillable>();

        foreach (var killable in killables)
        {
            killable.Kill();
        }
        
        killSFX.Play(this.transform.position);
        gameObject.SetActive(false);
    }
}
