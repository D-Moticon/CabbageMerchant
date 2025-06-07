using UnityEngine;
using System.Collections.Generic;

public abstract class BonkableSlotSpawner : MonoBehaviour
{
    //[HideInInspector]
    public List<BonkableSlot> bonkableSlots = new List<BonkableSlot>();
    public abstract void SpawnBonkableSlots();
    public abstract int GetTotalNumberStartingSlots();
    public PooledObjectData cabbageOverride;
}
