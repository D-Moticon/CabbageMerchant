using UnityEngine;

public abstract class CabbageVariant : MonoBehaviour
{
    [Header("Visuals and Effects")]
    public Material variantMaterial;
    public SFXInfo variantBonkSFX;
    public PooledObjectData variantHitVFX;

    protected Cabbage owningCabbage;

    public virtual void Initialize(Cabbage cabbage)
    {
        owningCabbage = cabbage;
        if (variantMaterial != null)
        {
            owningCabbage.sr.material = variantMaterial;
        }
    }

    public virtual void CabbageBonked(BonkParams bp)
    {
        variantBonkSFX.Play(owningCabbage.transform.position);
        if (variantHitVFX != null)
        {
            variantHitVFX.Spawn(owningCabbage.transform.position);
        }
        
    }

    public virtual void RemoveVariant()
    {
        gameObject.SetActive(false);
    }
}