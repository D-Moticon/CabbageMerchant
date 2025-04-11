using UnityEngine;

[CreateAssetMenu(fileName = "PooledObjectData", menuName = "Pooling/Pooled Object Data")]
public class PooledObjectData : ScriptableObject
{
    public GameObject prefab;

    public GameObject Spawn(Vector2 pos, Quaternion rot)
    {
        for (int i = 0; i < GameSingleton.Instance.objectPoolManager.pooledObjectsList.Count; i++)
        {
            if (GameSingleton.Instance.objectPoolManager.pooledObjectsList[i].objectData == this)
            {
                return GameSingleton.Instance.objectPoolManager.Spawn(this, pos, rot);
            }
        }

        return null;
    }
}