using System;
using UnityEngine;

public class OverworldBuildingEntranceTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public Vector2 spawnPos;
    public Vector2 targetPosDelta;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Singleton.Instance.overworldSceneChanger.ChangeScene(sceneToLoad, spawnPos, targetPosDelta);
    }
}
