using System;
using UnityEngine;
using TMPro;

public class BonkMarker : MonoBehaviour
{
    public PooledObjectData thisPooledObject;
    private Cabbage owningCabbage;
    private double bonkValue;
    public TMP_Text bonkValueText;
    public SFXInfo bonkSFX;
    public PooledObjectData bonkVFX;

    public delegate void MarkBonkDelegate(BonkParams bp);
    public static MarkBonkDelegate MarkedCabbageBonkedEvent;

    private Item owningItem;
    
    private void OnEnable()
    {
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
        GameStateMachine.ExitingScoringAction += ExitingScoringListener;
    }

    private void OnDisable()
    {
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
        GameStateMachine.ExitingScoringAction -= ExitingScoringListener;
    }

    public static BonkMarker MarkCabbage(Cabbage c, double bValue, Item item)
    {
        BonkMarker bm = Singleton.Instance.prefabReferences.bonkMarkerPrefab.Spawn(c.transform.position).GetComponent<BonkMarker>();
        bm.InitializeOnCabbage(c,bValue,item);
        return bm;
    }
    
    void InitializeOnCabbage(Cabbage c, double bValue, Item item)
    {
        this.transform.parent = c.transform;
        this.transform.localScale = Vector3.one;
        bonkValue = bValue;
        owningCabbage = c;
        owningItem = item;

        bonkValueText.text = Helpers.FormatWithSuffix(bValue);
    }
    
    private void CabbageBonkedListener(BonkParams bp)
    {
        if (bp.bonkedCabbage == null)
        {
            return;
        }
        
        if (bp.ball == null)
        {
            return;
        }

        if (bp.bonkedCabbage != owningCabbage)
        {
            return;
        }
        
        BonkParams markBP = new BonkParams();
        markBP.bonkerPower = bonkValue;
        markBP.collisionPos = transform.position;
        markBP.causingItem = owningItem;
        bp.bonkedCabbage.Bonk(markBP);
        
        MarkedCabbageBonkedEvent?.Invoke(markBP);
        
        Singleton.Instance.floaterManager.SpawnPopFloater(Helpers.FormatWithSuffix(bonkValue), bonkValueText.transform.position, bonkValueText.color);
        
        if (bonkVFX != null)
        {
            bonkVFX.Spawn(this.transform.position);
        }

        bonkSFX.Play(this.transform.position);
        GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
    }
    
    private void ExitingScoringListener()
    {
        GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
    }
}
