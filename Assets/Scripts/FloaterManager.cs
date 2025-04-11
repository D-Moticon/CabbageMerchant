using UnityEngine;
using System.Collections.Generic;

public class FloaterManager : MonoBehaviour
{
    [SerializeField] private Floater floaterPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private Queue<Floater> floaterPool;

    void Awake()
    {
        floaterPool = new Queue<Floater>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            Floater floater = Instantiate(floaterPrefab, transform);
            floater.Initialize(this);
            floater.gameObject.SetActive(false);
            floaterPool.Enqueue(floater);
        }
    }

    public void SpawnFloater(string text, Vector3 position, Color? textColor = null)
    {
        Floater floater;

        if (floaterPool.Count > 0)
        {
            floater = floaterPool.Dequeue();
        }
        else
        {
            floater = Instantiate(floaterPrefab, transform);
            floater.Initialize(this);
        }

        floater.transform.position = position;
        floater.gameObject.SetActive(true);
        floater.Activate(text, textColor);
    }

    public void ReturnFloater(Floater floater)
    {
        floater.gameObject.SetActive(false);
        floater.transform.SetParent(transform);
        floaterPool.Enqueue(floater);
    }
}