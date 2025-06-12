using System;
using UnityEngine;

public class HarvestButton : MonoBehaviour
{
    public PooledObjectData harvestVFX;
    
    private void OnMouseDown()
    {
        GameSingleton.Instance.gameStateMachine.Harvest();
        harvestVFX.Spawn(Vector2.zero);
        Singleton.Instance.uiManager.ShowNotification($"<color=green><wave a=.1>Early Harvest!");
    }
}
