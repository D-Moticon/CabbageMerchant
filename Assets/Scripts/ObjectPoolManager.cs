using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public PooledObjectData objectData;
        public int initialQuantity = 5;
        [Tooltip("Maximum allowed instances. 0 = unlimited")]
        public int maxQuantity = 0;
    }

    // internal per‐pool storage
    private class PoolData
    {
        public Queue<GameObject> available = new Queue<GameObject>();
        public List<GameObject> all       = new List<GameObject>();
        public List<GameObject> activeOrder = new List<GameObject>();
        public int maxQty;
    }

    [Tooltip("Configure each prefab, its initial fill, and max allowed.")]
    public List<PoolEntry> pooledObjectsList;

    private Dictionary<PooledObjectData, PoolData> poolDictionary;

    void Awake()
    {
        poolDictionary = new Dictionary<PooledObjectData, PoolData>();

        foreach (var entry in pooledObjectsList)
        {
            var data = new PoolData
            {
                maxQty = (entry.maxQuantity > 0)
                         ? entry.maxQuantity
                         : int.MaxValue
            };

            // pre‑populate
            for (int i = 0; i < entry.initialQuantity; i++)
            {
                var obj = Instantiate(entry.objectData.prefab, transform);
                obj.SetActive(false);
                data.available.Enqueue(obj);
                data.all.Add(obj);
            }

            poolDictionary.Add(entry.objectData, data);
        }
    }

    public GameObject Spawn(PooledObjectData objectData) =>
        Spawn(objectData, Vector3.zero, Quaternion.identity);

    public GameObject Spawn(PooledObjectData objectData,
                           Vector3 position,
                           Quaternion rotation)
    {
        if (!poolDictionary.TryGetValue(objectData, out var data))
        {
            Debug.LogWarning($"No pool for {objectData.name}, instantiating.");
            return Instantiate(objectData.prefab, position, rotation);
        }

        GameObject obj;

        if (data.available.Count > 0)
        {
            // reuse an inactive one
            obj = data.available.Dequeue();
        }
        else if (data.all.Count < data.maxQty)
        {
            // still under max → create new
            obj = Instantiate(objectData.prefab, transform);
            data.all.Add(obj);
        }
        else
        {
            // at max and none free → grab the oldest active
            if (data.activeOrder.Count > 0)
            {
                obj = data.activeOrder[0];
                data.activeOrder.RemoveAt(0);
                // (optionally reset any state here)
            }
            else
            {
                // fallback—should rarely happen
                obj = data.all[0];
            }
        }

        // position & activate
        obj.transform.SetParent(transform);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // track spawn order for reuse
        data.activeOrder.Add(obj);

        return obj;
    }

    public void ReturnToPool(PooledObjectData objectData, GameObject obj)
    {
        if (!poolDictionary.TryGetValue(objectData, out var data))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        data.available.Enqueue(obj);
        data.activeOrder.Remove(obj);
    }
}
