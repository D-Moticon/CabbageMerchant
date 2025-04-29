using UnityEngine;

[System.Serializable]
public class SpawnPooledObjectItemEffect : ItemEffect
{
    public PooledObjectData pooledObject;
    public string objectName;
    public string objectDescription;
    public Vector2 xRange = new Vector2(-5f, 5f);
    public Vector2 yRange = new Vector2(-5f, 5f);
    public int quantity = 1;
    public float scale = 1f;
    
    public override void TriggerItemEffect(TriggerContext tc)
    {
        for (int i = 0; i < quantity; i++)
        {
            float xRand = Random.Range(xRange.x, xRange.y);
            float yRand = Random.Range(yRange.x, yRange.y);
            Vector2 pos = new Vector2(xRand, yRand);
            GameObject go = pooledObject.Spawn(pos);
            go.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public override string GetDescription()
    {
        string plural = "";
        if (quantity > 1)
        {
            plural = "s";
        }

        return ($"Spawn {quantity} {objectName}{plural} that {objectDescription}");
    }
    
    public override void RandomizePower()
    {
        base.RandomizePower();
        quantity = Random.Range(1, 8);
    }
}
