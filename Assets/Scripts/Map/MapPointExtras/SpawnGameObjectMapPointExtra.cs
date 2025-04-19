using UnityEngine;

[System.Serializable]
public class SpawnGameObjectMapPointExtra : MapPointExtra
{
    public GameObject objectToSpawn;
    public Vector2 spawnPos = Vector2.zero;
    public override void GenerateMapPointExtra()
    {
        GameObject.Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
    }
}
