using UnityEngine;
using System.Collections.Generic;

public class FloaterManager : MonoBehaviour
{
    [Header("Floater References (ScriptableObjects)")]
    public List<FloaterReference> floaterReferences;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 10;

    // Internal dictionary mapping a FloaterReference -> its pool of Floater instances
    private Dictionary<FloaterReference, Queue<Floater>> floaterPools;

    void Awake()
    {
        floaterPools = new Dictionary<FloaterReference, Queue<Floater>>();

        // Pre-populate pools for each FloaterReference
        foreach (var floaterRef in floaterReferences)
        {
            CreatePoolForReference(floaterRef);
        }
    }

    private void CreatePoolForReference(FloaterReference floaterRef)
    {
        var queue = new Queue<Floater>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            Floater floater = Instantiate(floaterRef.floaterPrefab, transform);
            floater.Initialize(this);
            floater.gameObject.SetActive(false);
            queue.Enqueue(floater);
        }

        floaterPools[floaterRef] = queue;
    }

    /// <summary>
    /// Spawns a floater of the given reference at the specified position.
    /// </summary>
    /// <param name=\"floaterRef\">Which FloaterReference to use (i.e. which prefab)</param>
    /// <param name=\"text\">Text content displayed by the floater</param>
    /// <param name=\"position\">World space position to spawn at</param>
    /// <param name=\"textColor\">Optional color for the text</param>
    /// <param name=\"scale\">Optional scale for the Floater</param>
    public void SpawnFloater(FloaterReference floaterRef, string text, Vector3 position, Color? textColor = null, float scale = 1f)
    {
        // If there's no pool for this reference yet, create one on the fly
        if (!floaterPools.ContainsKey(floaterRef))
        {
            CreatePoolForReference(floaterRef);
        }

        Queue<Floater> pool = floaterPools[floaterRef];
        Floater floater;

        if (pool.Count > 0)
        {
            floater = pool.Dequeue();
        }
        else
        {
            // Expand the pool if exhausted
            floater = Instantiate(floaterRef.floaterPrefab, transform);
            floater.Initialize(this);
        }

        floater.transform.position = position;
        floater.transform.localScale = new Vector3(scale, scale, scale);
        floater.gameObject.SetActive(true);
        floater.Activate(text, textColor);
    }

    /// <summary>
    /// Return a floater to the pool it came from. If that FloaterReference
    /// wasn't tracked, it creates a new queue for it.
    /// </summary>
    public void ReturnFloater(Floater floater, FloaterReference floaterRef)
    {
        if (!floaterPools.ContainsKey(floaterRef))
        {
            floaterPools[floaterRef] = new Queue<Floater>();
        }

        floater.gameObject.SetActive(false);
        floater.transform.SetParent(transform);
        floaterPools[floaterRef].Enqueue(floater);
    }

    public void SpawnInfoFloater(string text, Vector3 position, Color? textColor = null, float scale = 1f)
    {
        SpawnFloater(floaterReferences[3], text, position, textColor, scale);
    }
}
