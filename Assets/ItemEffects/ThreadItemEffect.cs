using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ThreadItemEffect", menuName = "ItemEffects/ThreadItemEffect")]
public class ThreadItemEffect : ItemEffect
{
    [Tooltip("Line renderer prefab for drawing thread segments.")]
    public PooledObjectData threadLinePrefab;
    [Tooltip("Spark effect to propagate along the thread.")]
    public PooledObjectData sparkPrefab;

    public float bonkValue = 0.1f;

    private List<Cabbage> chain = new List<Cabbage>();
    private List<LineRenderer> segmentRenderers = new List<LineRenderer>();
    private bool chainActive = false;

    public override void InitializeItemEffect()
    {
        // start listening when bounce state enters
        GameStateMachine.EnteringBounceStateAction += OnBounceStateEntered;
        // listen to cabbage hits
        Ball.BallHitBonkableEvent += OnBallHitCabbage;
    }

    public override void DestroyItemEffect()
    {
        GameStateMachine.EnteringBounceStateAction -= OnBounceStateEntered;
        Ball.BallHitBonkableEvent -= OnBallHitCabbage;
    }

    private void OnBounceStateEntered()
    {
        return;
        // reset chain at start of first ball
        chain.Clear();
        // remove any existing visuals
        foreach (var lr in segmentRenderers)
            lr.gameObject.SetActive(false);
        segmentRenderers.Clear();
        chainActive = false;
    }

    private void OnBallHitCabbage(Ball.BallHitBonkableParams bhcp)
    {
        var hitCabbage = bhcp.cabbage;
        if (!chainActive)
        {
            // first hit initializes chain
            chain.Add(hitCabbage);
            chainActive = true;
        }
        else
        {
            // subsequent hits, if new, append to chain and draw segment
            if (!chain.Contains(hitCabbage))
            {
                var prev = chain.Last();
                chain.Add(hitCabbage);
                // spawn a pooled line renderer between prev and hit
                var lineObj = threadLinePrefab.Spawn();
                var lr = lineObj.GetComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, prev.transform.position);
                lr.SetPosition(1, hitCabbage.transform.position);
                segmentRenderers.Add(lr);
            }
        }
    }

    public override void TriggerItemEffect(TriggerContext tc)
    {
        // called on subsequent balls when trigger happens
        // if a cabbage in chain is hit, propagate
        Cabbage hit = tc.cabbage; // assume context provides cabbage
        if (!chainActive || !chain.Contains(hit))
            return;

        // find index
        int idx = chain.IndexOf(hit);
        // propagate forward and backward
        var targets = new List<int>();
        if (idx > 0) targets.Add(idx - 1);
        if (idx < chain.Count - 1) targets.Add(idx + 1);

        // prevent loops: track visited
        HashSet<int> visited = new HashSet<int> { idx };
        foreach (var tIdx in targets)
            GameSingleton.Instance.gameStateMachine.StartCoroutine(PropagateSpark(idx, tIdx, visited));
    }

    private IEnumerator PropagateSpark(int fromIdx, int toIdx, HashSet<int> visited)
    {
        var from = chain[fromIdx];
        var to = chain[toIdx];
        // instantiate spark at 'from'
        var spark = sparkPrefab.Spawn();
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            spark.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, t);
            yield return null;
        }
        // bonk the 'to' cabbage
        BonkParams bp = new BonkParams();
        bp.bonkerPower = bonkValue;
        to.Bonk(bp);
        // continue propagation
        visited.Add(toIdx);
        var next = new List<int>();
        if (toIdx > 0 && !visited.Contains(toIdx - 1)) next.Add(toIdx - 1);
        if (toIdx < chain.Count - 1 && !visited.Contains(toIdx + 1)) next.Add(toIdx + 1);
        foreach (var n in next)
            yield return PropagateSpark(toIdx, n, visited);
        // despawn spark
        spark.gameObject.SetActive(false);
    }

    public override string GetDescription()
    {
        return "Thread: chains cabbages on first hit, propagates bonk through chain on subsequent balls.";
    }
}
