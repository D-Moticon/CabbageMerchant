using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using TMPro;

public class ChaosCabbageBonkable : MonoBehaviour
{
    public Cabbage c;
    public ChaosCabbageSO ccso;
    public SFXInfo destroySFX;
    public PooledObjectData destroyVFX;
    
    
    private void OnEnable()
    {
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
    }

    private void OnDisable()
    {
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
    }

    void CabbageBonkedListener(BonkParams bp)
    {
        if (bp.bonkedCabbage != c)
        {
            return;
        }

        if (c.points <= 0)
        {
            PetDefinition pd = Singleton.Instance.petManager.currentPet;
            Singleton.Instance.chaosManager.CollectChaosCabbageFromDef(ccso);
            destroySFX.Play();
            if (destroyVFX != null)
            {
                destroyVFX.Spawn(this.transform.position);
            }
            c.Remove();
            if (GameSingleton.Instance != null)
            {
                GameSingleton.Instance.gameStateMachine.ForceEndRound();
            }
        }
    }
}
