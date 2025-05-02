using UnityEngine;

public class TransformIntoItem : ItemEffect
{
    public ItemCollection itemCollection;
    public bool forceHolofoil = false;
    public PooledObjectData spawnVFX;
    public SFXInfo spawnSFX;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        Item newItemPrefab = itemCollection.GetRandomItem();
        Item newItem = Singleton.Instance.itemManager.GenerateItemWithWrapper(newItemPrefab, owningItem.transform.position);
        ItemSlot itemSlot = owningItem.currentItemSlot;
        owningItem.DestroyItem();
        Singleton.Instance.itemManager.AddItemToSlot(newItem, itemSlot);
        if (forceHolofoil)
        {
            newItem.SetHolofoil();
        }

        if (spawnVFX != null)
        {
            spawnVFX.Spawn(newItem.transform.position);
            spawnSFX.Play();
        }
    }

    public override string GetDescription()
    {
        return "Transform into a random item";
    }
}
