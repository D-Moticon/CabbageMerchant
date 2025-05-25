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
    public PooledObjectData bonkMarkerVFX;

    public delegate void MarkBonkDelegate(BonkParams bp);
    public static MarkBonkDelegate MarkedCabbageBonkedEvent;

    private Item owningItem;
    
    private void OnEnable()
    {
        Cabbage.CabbageBonkedEvent += CabbageBonkedListener;
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
        Cabbage.CabbagePoppedEvent += CabbagePoppedListener;
        
        GameStateMachine.ExitingScoringAction += ExitingScoringListener;
    }

    private void OnDisable()
    {
        Cabbage.CabbageBonkedEvent -= CabbageBonkedListener;
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
        Cabbage.CabbagePoppedEvent -= CabbagePoppedListener;
        GameStateMachine.ExitingScoringAction -= ExitingScoringListener;
    }

    private void Update()
    {
        if (owningCabbage != null)
        {
            transform.position = owningCabbage.transform.position;
            //transform.localScale = owningCabbage.transform.localScale;
            
            if (!owningCabbage.gameObject.activeInHierarchy)
            {
                owningCabbage = null;
                GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
            }
        }

        

        else
        {
            GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
        }
    }

    public static BonkMarker MarkCabbage(Cabbage c, double bValue, Item item)
    {
        BonkMarker bm = Singleton.Instance.prefabReferences.bonkMarkerPrefab.Spawn(c.transform.position).GetComponent<BonkMarker>();
        bm.InitializeOnCabbage(c,bValue,item);
        return bm;
    }
    
    void InitializeOnCabbage(Cabbage c, double bValue, Item item)
    {
        //this.transform.parent = c.transform;
        this.transform.position = c.transform.position;
        //this.transform.localScale = c.transform.localScale;
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
        
        if (bp.ball == null && !bp.forceMarkBonk)
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

        if (bonkMarkerVFX != null)
        {
            GameObject bvfx = bonkMarkerVFX.Spawn(this.transform.position);
            bvfx.transform.localScale = this.transform.localScale;
        }

        bonkSFX.Play(this.transform.position);
        GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
    }
    
    private void ExitingScoringListener()
    {
        GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
    }

    void TransferMark(Cabbage newCabbage)
    {
        MarkCabbage(newCabbage, bonkValue, owningItem);
        GameSingleton.Instance.objectPoolManager.ReturnToPool(thisPooledObject, this.gameObject);
    }
    
    private void CabbageMergedListener(Cabbage.CabbageMergedParams cpp)
    {
        if (cpp.oldCabbageA == owningCabbage || cpp.oldCabbageB == owningCabbage)
        {
            TransferMark(cpp.newCabbage);
        }
    }
    
    private void CabbagePoppedListener(Cabbage.CabbagePoppedParams cpp)
    {
        if (cpp.c != owningCabbage)
        {
            return;
        }

        Cabbage c = GameSingleton.Instance.gameStateMachine.GetRandomActiveCabbage();
        if (c != null)
        {
            TransferMark(c);
        }
        
    }

}
