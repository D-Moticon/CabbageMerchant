using UnityEngine;
using System.Collections.Generic;

public class FloaterManager : MonoBehaviour
{
    [Header("Floater References (ScriptableObjects)")]
    public List<FloaterReference> floaterReferences;

    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 10;

    // Inactive‐pool per reference
    private Dictionary<FloaterReference, Queue<Floater>> floaterPools;
    // All created floaters per reference (for circular reuse)
    private Dictionary<FloaterReference, List<Floater>> floaterAll;
    // Next index into floaterAll when pool is exhausted
    private Dictionary<FloaterReference, int> nextReuseIndex;

    void Awake()
    {
        floaterPools      = new Dictionary<FloaterReference, Queue<Floater>>();
        floaterAll        = new Dictionary<FloaterReference, List<Floater>>();
        nextReuseIndex    = new Dictionary<FloaterReference, int>();

        // Pre‐populate pools for each FloaterReference
        foreach (var floaterRef in floaterReferences)
        {
            CreatePoolForReference(floaterRef);
        }
    }

    private void CreatePoolForReference(FloaterReference floaterRef)
    {
        var queue = new Queue<Floater>(initialPoolSize);
        var all   = new List<Floater>(initialPoolSize);

        for (int i = 0; i < initialPoolSize; i++)
        {
            Floater floater = Instantiate(floaterRef.floaterPrefab, transform);
            floater.Initialize(this);
            floater.gameObject.SetActive(false);

            queue.Enqueue(floater);
            all.  Add(floater);
        }

        floaterPools   [floaterRef] = queue;
        floaterAll     [floaterRef] = all;
        nextReuseIndex[floaterRef] = 0;
    }

    /// <summary>
    /// Spawns a floater of the given reference at the specified position.
    /// If the pool is empty, reuses the oldest created floater instead of instantiating.
    /// </summary>
    public void SpawnFloater(
        FloaterReference floaterRef,
        string           text,
        Vector3          position,
        Color?           textColor = null,
        float            scale     = 1f)
    {
        // Ensure we have a pool
        if (!floaterPools.ContainsKey(floaterRef))
            CreatePoolForReference(floaterRef);

        var pool = floaterPools[floaterRef];
        Floater floater;

        if (pool.Count > 0)
        {
            // normal path: pull an inactive one
            floater = pool.Dequeue();
        }
        else
        {
            // pool exhausted ⇒ reuse the oldest created
            var allList = floaterAll[floaterRef];
            int idx     = nextReuseIndex[floaterRef];
            floater     = allList[idx];

            // advance circularly
            nextReuseIndex[floaterRef] = (idx + 1) % allList.Count;
        }

        // configure & activate
        floater.transform.position    = position;
        floater.transform.localScale   = Vector3.one * scale;
        floater.gameObject.SetActive(true);
        floater.Activate(text, textColor);
    }

    /// <summary>
    /// Return a floater to the inactive‐pool.
    /// </summary>
    public void ReturnFloater(Floater floater, FloaterReference floaterRef)
    {
        // lazily ensure a pool exists
        if (!floaterPools.ContainsKey(floaterRef))
            CreatePoolForReference(floaterRef);

        floater.gameObject.SetActive(false);
        floater.transform.SetParent(transform);
        floaterPools[floaterRef].Enqueue(floater);
    }

    public void SpawnInfoFloater(
        string text,
        Vector3 position,
        Color? textColor = null,
        float scale      = 1f)
    {
        float randPosMult = 0.2f;
        float randX = Random.Range(-scale, scale) * randPosMult;
        float randY = Random.Range(-scale, scale) * randPosMult;
        Vector2 randPos = new Vector2(randX, randY);

        // assuming index 3 is your info‐floater reference
        var infoRef = floaterReferences[3];
        SpawnFloater(infoRef, text, position + (Vector3)randPos, textColor, scale);
    }
}
