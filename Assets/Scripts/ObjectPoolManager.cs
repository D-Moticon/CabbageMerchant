using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public PooledObjectData objectData;
        public int initialQuantity;
    }

    public List<PoolEntry> pooledObjectsList;

    private Dictionary<PooledObjectData, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        poolDictionary = new Dictionary<PooledObjectData, Queue<GameObject>>();

        foreach (var entry in pooledObjectsList)
        {
            var objectQueue = new Queue<GameObject>();

            for (int i = 0; i < entry.initialQuantity; i++)
            {
                GameObject obj = Instantiate(entry.objectData.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(entry.objectData, objectQueue);
        }
    }

    public GameObject Spawn(PooledObjectData objectData)
    {
        return Spawn(objectData, Vector3.zero, Quaternion.identity);
    }
    
    public GameObject Spawn(PooledObjectData objectData, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(objectData))
        {
            Debug.LogWarning($"No pool exists for {objectData.name}, instantiating a new object.");
            return Instantiate(objectData.prefab, position, rotation);
        }

        Queue<GameObject> objectQueue = poolDictionary[objectData];
        GameObject spawnedObj;

        if (objectQueue.Count > 0)
        {
            spawnedObj = objectQueue.Dequeue();
        }
        else
        {
            spawnedObj = Instantiate(objectData.prefab, transform);
        }

        spawnedObj.transform.position = position;
        spawnedObj.transform.rotation = rotation;
        spawnedObj.SetActive(true);

        return spawnedObj;
    }

    public void ReturnToPool(PooledObjectData objectData, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        
        if (!poolDictionary.ContainsKey(objectData))
        {
            poolDictionary[objectData] = new Queue<GameObject>();
        }

        poolDictionary[objectData].Enqueue(obj);
    }
}
