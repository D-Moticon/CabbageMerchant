using UnityEngine;

public class SpawnItemInEmptyCabbageSlotEffect : ItemEffect
{
    public PooledObjectData objectToSpawn;
    public string objectName;
    public bool ensureNoCabbageOverlap = true;
    public float overlapCheckRadius = 0.5f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        GameStateMachine.CabbageSlot cs =
            GameSingleton.Instance.gameStateMachine.GetEmptyCabbageSlot(ensureNoCabbageOverlap, overlapCheckRadius);
        if (cs == null)
        {
            return;
        }

        Vector2 pos = cs.position;
        objectToSpawn.Spawn(pos);
        
    }

    public override string GetDescription()
    {
        return ($"Spawn {objectName} in a random open space");
    }
}
