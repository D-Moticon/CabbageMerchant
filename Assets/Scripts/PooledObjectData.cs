using UnityEngine;

[CreateAssetMenu(fileName = "PooledObjectData", menuName = "Pooling/Pooled Object Data")]
public class PooledObjectData : ScriptableObject
{
    public GameObject prefab;

    public GameObject Spawn(Vector2 pos = default, Quaternion rot = default)
    {
        if (Singleton.Instance != null)
        {
            for (int i = 0; i < Singleton.Instance.objectPoolManager.pooledObjectsList.Count; i++)
            {
                if (Singleton.Instance.objectPoolManager.pooledObjectsList[i].objectData == this)
                {
                    return Singleton.Instance.objectPoolManager.Spawn(this, pos, rot);
                }
            }
        }
        
        if (GameSingleton.Instance != null)
        {
            for (int i = 0; i < GameSingleton.Instance.objectPoolManager.pooledObjectsList.Count; i++)
            {
                if (GameSingleton.Instance.objectPoolManager.pooledObjectsList[i].objectData == this)
                {
                    return GameSingleton.Instance.objectPoolManager.Spawn(this, pos, rot);
                }
            }
        }
        
        

        return null;
    }
}