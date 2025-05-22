using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using TMPro;

public class Enemy : MonoBehaviour, IBonkable
{
    public float maxHP = 10f;
    [HideInInspector] public float hp;
    public MMF_Player bonkFeel;
    public PooledObjectData bonkVFX;
    public SFXInfo bonkSFX;
    public PooledObjectData killVFX;
    public SFXInfo killSFX;
    public TMP_Text hpText;

    private void OnEnable()
    {
        hp = maxHP;
        if (hpText != null)
        {
            hpText.text = $"{hp:F0}";
        }
    }

    private void OnDisable()
    {
        //throw new NotImplementedException();
    }

    public void Bonk(BonkParams bp)
    {
        hp -= (float)bp.bonkerPower;
        
        if (hpText != null)
        {
            hpText.text = $"{hp:F0}";
        }
        
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

    public GameObject GetGameObject()
    {
        return gameObject;
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
