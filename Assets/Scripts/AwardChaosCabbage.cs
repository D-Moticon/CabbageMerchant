using System;
using UnityEngine;

public class AwardChaosCabbage : MonoBehaviour
{
    public Cabbage cabbage;
    public ChaosCabbageSO chaosCabbage;
    public PooledObjectData destroyVFX;
    public SFXInfo destroySFX;
    
    private void OnEnable()
    {
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
    }

    private void OnDisable()
    {
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
    }
    
    private void CabbageBonkedListener(BonkParams bp)
    {
        if (bp.bonkedCabbage != cabbage)
        {
            return;
        }

        if (cabbage.points <= 0.001)
        {
            destroySFX.Play();
            if (destroyVFX != null)
            {
                destroyVFX.Spawn(this.transform.position);
            }
            cabbage.Remove();
            if (GameSingleton.Instance != null)
            {
                GameSingleton.Instance.gameStateMachine.CollectChaosCabbage(chaosCabbage);
            }
        }
    }

    public void SetChaosCabbage(ChaosCabbageSO ccso)
    {
        chaosCabbage = ccso;
        GetComponentInChildren<SpriteRenderer>().sprite = ccso.item.icon;
    }
}
