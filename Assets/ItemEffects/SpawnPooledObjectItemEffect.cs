using UnityEngine;

[System.Serializable]
public class SpawnPooledObjectItemEffect : ItemEffect
{
    public PooledObjectData pooledObject;
    public Vector2 xRange = new Vector2(-5f, 5f);
    public Vector2 yRange = new Vector2(-5f, 5f);
    public int quantity = 1;
    
    public override void TriggerItemEffect()
    {
        for (int i = 0; i < quantity; i++)
        {
            float xRand = Random.Range(xRange.x, xRange.y);
            float yRand = Random.Range(yRange.x, yRange.y);
            Vector2 pos = new Vector2(xRand, yRand);
            pooledObject.Spawn(pos);
        }
    }
}
