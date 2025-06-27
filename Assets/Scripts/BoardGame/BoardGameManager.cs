using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Splines;
using Random = UnityEngine.Random;
using TMPro;
using System.Linq;

public class BoardGameManager : MonoBehaviour
{
    public SplineContainer splineContainer;
    public BoardGameSquare boardGameSquarePrefab;
    public BoardGameSquareSO firstSquareSO;
    public BoardGameSquareSO lastSquareSO;
    public Transform squareParent;
    public LineRenderer lineRendererPrefab;  // assign your prefab with desired material
    public float spawnDelay = 0.1f;
    public BoardGameBucket bucketPrefab;
    public Transform bucketParent;
    public Transform bucketSpawnStart;
    public float bucketSpacing = 1.5f;
    public PooledObjectData bucketHitVFX;
    public SFXInfo bucketHitSFX;
    public GameObject tokenObject;
    public GameObject boardUIRoot;
    public GameObject boardShowButton;
    public GameObject rollButton;
    private bool boardVisible;
    
    [Header("Move-Count UI")]
    public AnimationCurve hopCurve = AnimationCurve.EaseInOut(0,0,1,0);

    public float hopAmount = 0.2f;
    public Canvas       uiCanvas;          // the Canvas for board-game UI
    public TMP_Text moveCountText;  // assign in inspector
    public float        floatDuration = 1f; // time to float from bucket → center
    private Coroutine   _moveCounterRoutine;
    private Vector3     _lastBucketWorldPos;

    private List<BoardGameSquare> instantiatedSquares = new List<BoardGameSquare>();
    private int currentTileIndex = 0;

    private bool bucketAlreadyHit = false;
    private List<BoardGameBucket> activeBuckets = new List<BoardGameBucket>();

    public bool boardGameActive = false;
    
    [System.Serializable]
    public class SquareInfo
    {
        public BoardGameSquareSO squareSO;
        public float weight = 1f;
    }
    
    private void OnEnable()
    {
        tokenObject.GetComponentInChildren<SpriteRenderer>().sprite =
            Singleton.Instance.skinManager.currentSkin.downSprite;
        moveCountText.gameObject.SetActive(false);
        rollButton.SetActive(false);
        if(!boardGameActive) gameObject.SetActive(false);
    }

    public IEnumerator GenerateBoard(List<SquareInfo> squareInfos, int numberSquares)
    {
        gameObject.SetActive(true);
        rollButton.SetActive(false);
        instantiatedSquares.Clear();
        currentTileIndex = 0;
        ShowBoard();

        LineRenderer fullPath = null;
        if (lineRendererPrefab != null)
        {
            fullPath = Instantiate(lineRendererPrefab, squareParent);
            fullPath.useWorldSpace = true;
            fullPath.positionCount = 0;
        }

        var spline = splineContainer.Spline;
        if (spline == null || spline.Count == 0)
            yield break;

        for (int i = 0; i < numberSquares; i++)
        {
            float t = Mathf.Clamp01((float)i / (numberSquares - 1));
            Vector3 pos = splineContainer.EvaluatePosition(t);
            Quaternion rot = Quaternion.identity;

            BoardGameSquareSO chosenSO;

            if (i == 0 && firstSquareSO != null)
                chosenSO = firstSquareSO;
            else if (i == numberSquares - 1 && lastSquareSO != null)
                chosenSO = lastSquareSO;
            else
                chosenSO = WeightedRandom(squareInfos);

            var square = Instantiate(boardGameSquarePrefab, pos, rot, squareParent);
            square.SetSquareData(chosenSO);
            square.PlayHopFeel();
            instantiatedSquares.Add(square);

            if (fullPath != null)
            {
                fullPath.positionCount = instantiatedSquares.Count;
                for (int j = 0; j < instantiatedSquares.Count; j++)
                    fullPath.SetPosition(j, instantiatedSquares[j].transform.position);
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        if (tokenObject != null && instantiatedSquares.Count > 0)
            tokenObject.transform.position = instantiatedSquares[0].transform.position;

        rollButton.SetActive(true);
    }

    private BoardGameSquareSO WeightedRandom(List<SquareInfo> squareInfos)
    {
        float totalWeight = squareInfos.Sum(si => si.weight);
        float randomPoint = Random.value * totalWeight;

        foreach (var info in squareInfos)
        {
            if (randomPoint < info.weight)
                return info.squareSO;

            randomPoint -= info.weight;
        }

        return squareInfos[^1].squareSO; // Fallback
    }

    private BoardGameSquareSO GetRandomFromPool(List<BoardGameSquareSO> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        int rand = Random.Range(0, pool.Count);
        return pool[rand];
    }

    public int GetCurrentTileIndex()
    {
        return 0;
    }

    public BoardGameSquareSO GetSquareAt(int index)
    {
        return null;
    }

    public void ShowBoard()
    {
        boardUIRoot.SetActive(true);
        boardVisible = true;
        GameSingleton.Instance.gameStateMachine.allowFiring = false;
        boardShowButton.SetActive(false);
    }

    public void HideBoard()
    {
        boardUIRoot.SetActive(false);
        boardVisible = false;
        StartCoroutine(EnableFiringRoutine()); // need a delay to prevent bug where the ball is immediately fired when closing the board
        boardShowButton.SetActive(true);
    }

    IEnumerator EnableFiringRoutine()
    {
        yield return null;
        GameSingleton.Instance.gameStateMachine.allowFiring = true;
    }
    
    public void ToggleBoardVisibility()
    {
        if (boardUIRoot == null)
            return;

        if (!boardVisible)
        {
            ShowBoard();
        }

        if (boardVisible)
        {
            HideBoard();
        }
    }
    
    public void OnBucketHit(BoardGameBucket bucket)
    {
        if (bucketAlreadyHit) return;
        bucketAlreadyHit = true;

        // kill balls, show board, etc.
        GameSingleton.Instance.gameStateMachine.KillAllBalls();

        // store for UI
        _lastBucketWorldPos = bucket.transform.position;

        if (bucketHitVFX != null)
        {
            bucketHitVFX.Spawn(_lastBucketWorldPos);
        }

        bucketHitSFX.Play(_lastBucketWorldPos);
        
        // start the display+move+countdown
        _moveCounterRoutine = StartCoroutine(ShowAndMoveThenToken(bucket.moveAmount) );
    }
    
    IEnumerator ShowAndMoveThenToken(int steps)
    {
        rollButton.SetActive(false);
        
        // 1) snap UI to bucket
        Vector2 startAnchored = WorldToCanvasPosition(_lastBucketWorldPos);
        moveCountText.rectTransform.anchoredPosition = startAnchored;
        moveCountText.text = steps.ToString();
        moveCountText.gameObject.SetActive(true);

        // 2) float up to center
        yield return StartCoroutine( FloatToCenter(startAnchored) );
        
        yield return new WaitForSeconds(1f);
        ShowBoard();

        // 3) walk the token, counting down on UI
        yield return StartCoroutine( MoveTokenWithCountdown(steps) );

        // 4) hide the text
        moveCountText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(Singleton.Instance.dialogueManager.DialogueTaskRoutine(instantiatedSquares[currentTileIndex].squareData.onLandedTasks));
        bool reachedEnd = (currentTileIndex >= instantiatedSquares.Count - 1);

        if (reachedEnd)
        {
            // Tell BossFightManager to continue to the next phase
            foreach (var bucket in activeBuckets)
            {
                if (bucket != null)
                    Destroy(bucket.gameObject);
            }
            activeBuckets.Clear();
            HideBoard();
            boardShowButton.SetActive(false);
            yield return StartCoroutine(Singleton.Instance.bossFightManager.FinishPhaseRoutine(loopPhase: false));
        }
        else
        {
            // Stay in current phase, repeat it
            yield return StartCoroutine(Singleton.Instance.bossFightManager.FinishPhaseRoutine(loopPhase: true));
        }
        
        rollButton.SetActive(true);
    }
    
    IEnumerator FloatToCenter(Vector2 from)
    {
        Vector2 to = Vector2.zero; // center of canvas

        float  t = 0f;
        while (t < floatDuration)
        {
            t += Time.deltaTime;
            moveCountText.rectTransform.anchoredPosition =
                Vector2.Lerp(from, to, t/floatDuration);
            yield return null;
        }
        moveCountText.rectTransform.anchoredPosition = to;
    }
    
    IEnumerator MoveTokenWithCountdown(int steps)
    {
        int remaining = steps;
        for (int i = 0; i < steps; i++)
        {
            // assume your MoveToken does one step at a time
            yield return StartCoroutine(MoveTokenStep() );

            remaining--;
            moveCountText.text = remaining.ToString();
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator MoveTokenStep()
    {
        int next = Mathf.Min(currentTileIndex + 1, instantiatedSquares.Count - 1);
        Vector3 start = tokenObject.transform.position;
        Vector3 end   = instantiatedSquares[next].transform.position;

        float elapsed = 0f;
        const float duration = 0.2f;    // your existing per‐step duration
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // linear move
            Vector3 basePos = Vector3.Lerp(start, end, t);
            // vertical hop
            float yOff = hopCurve.Evaluate(t)*hopAmount;

            tokenObject.transform.position = basePos + Vector3.up * yOff;
            yield return null;
        }

        // snap final
        tokenObject.transform.position = end;
        currentTileIndex = next;
        
        instantiatedSquares[next].PlayHopFeel();

        // small pause before next hop
        yield return new WaitForSeconds(0.05f);
    }
    
    Vector2 WorldToCanvasPosition(Vector3 worldPos)
    {
        var canvasRT = uiCanvas.GetComponent<RectTransform>();
        // Convert world → local space of the canvas
        Vector3 local = canvasRT.InverseTransformPoint(worldPos);
        return local;
    }
    
    public IEnumerator SpawnBuckets(List<int> moveAmounts)
    {
        // Clean up any previous
        foreach (var b in activeBuckets)
            if (b != null) Destroy(b.gameObject);
        activeBuckets.Clear();
        bucketAlreadyHit = false;
        
        moveAmounts.Shuffle();

        for (int i = 0; i < moveAmounts.Count; i++)
        {
            Vector3 spawnPos = bucketSpawnStart.position + new Vector3(i * bucketSpacing, 0f, 0f);
            BoardGameBucket bucket = Instantiate(bucketPrefab, spawnPos, Quaternion.identity, bucketParent);
            bucket.SetMoveNumber(moveAmounts[i]);
            activeBuckets.Add(bucket);

            yield return null; // stagger spawn if desired
        }
    }
    
    
}
