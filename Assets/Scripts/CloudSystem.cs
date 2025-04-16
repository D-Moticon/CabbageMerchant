using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudSystem : MonoBehaviour
{
    [Header("Cloud Prefabs (must have a SpriteRenderer)")]
    public List<GameObject> cloudPrefabs;

    [Header("Pool Settings")]
    [Tooltip("How many total cloud objects to pre‑instantiate.")]
    public int poolSize = 20;

    [Header("Spawn Settings")]
    [Tooltip("Clouds spawned per second.")]
    public float spawnRate = 0.5f;
    [Tooltip("Overall speed multiplier for cloud movement.")]
    public float globalSpeed = 1f;
    [Tooltip("How far off‑screen (in world units) to the left new clouds appear.")]
    public float spawnOffsetX = 1f;

    [Header("Spawn Y Range")]
    [Tooltip("Minimum world‑space Y at which clouds can spawn.")]
    public float spawnYMin = 0f;
    [Tooltip("Maximum world‑space Y at which clouds can spawn.")]
    public float spawnYMax = 10f;

    [Header("Parallax / Scale")]
    [Tooltip("Scale at bottom of spawn range.")]
    public float minScale = 0.5f;
    [Tooltip("Scale at top of spawn range.")]
    public float maxScale = 1.5f;

    [Header("Parallax / Speed")]
    [Tooltip("Speed at bottom of spawn range.")]
    public float minSpeed = 0.2f;
    [Tooltip("Speed at top of spawn range.")]
    public float maxSpeed = 1f;

    [Header("Fog / Haze Colors")]
    [Tooltip("Near‑cloud (foreground) color.")]
    public Color foregroundColor = Color.white;
    [Tooltip("Far‑cloud (background) color.)")]
    public Color backgroundColor = Color.gray;

    // internal
    private List<CloudBehavior> _pool;
    private float _leftEdge, _rightEdge;

    void Start()
    {
        // Compute camera bounds in world space
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width  = height * cam.aspect;
        Vector3 cp   = cam.transform.position;

        _leftEdge   = cp.x - width  / 2f;
        _rightEdge  = cp.x + width  / 2f;

        // Clamp spawnYMin/YMax to camera bounds
        float bottomEdge = cp.y - height / 2f;
        float topEdge    = cp.y + height / 2f;
        spawnYMin = Mathf.Clamp(spawnYMin, bottomEdge, topEdge);
        spawnYMax = Mathf.Clamp(spawnYMax, bottomEdge, topEdge);
        if (spawnYMin > spawnYMax)
        {
            // swap if inverted
            float tmp = spawnYMin;
            spawnYMin = spawnYMax;
            spawnYMax = tmp;
        }

        // Pre‑instantiate pool
        _pool = new List<CloudBehavior>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var prefab = cloudPrefabs[ Random.Range(0, cloudPrefabs.Count) ];
            var go     = Instantiate(prefab, transform);
            go.SetActive(false);
            var cb     = go.AddComponent<CloudBehavior>();
            _pool.Add(cb);
        }

        // Prepopulate the sky
        PrepopulateClouds();

        // Kick off the spawn loop
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Draws the spawn Y range in the scene view when selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float height = 2f * cam.orthographicSize;
        float width  = height * cam.aspect;
        Vector3 cp   = cam.transform.position;
        float left   = cp.x - width  / 2f;
        float right  = cp.x + width  / 2f;

        // Draw horizontal lines at spawnYMin and spawnYMax
        Gizmos.color = Color.yellow;
        Vector3 yMinStart = new Vector3(left, spawnYMin, 0f);
        Vector3 yMinEnd   = new Vector3(right, spawnYMin, 0f);
        Gizmos.DrawLine(yMinStart, yMinEnd);

        Vector3 yMaxStart = new Vector3(left, spawnYMax, 0f);
        Vector3 yMaxEnd   = new Vector3(right, spawnYMax, 0f);
        Gizmos.DrawLine(yMaxStart, yMaxEnd);

        // Optionally, draw a wire rectangle for the spawn band
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Vector3 center = new Vector3(cp.x, (spawnYMin + spawnYMax) * 0.5f, 0f);
        Vector3 size   = new Vector3(width, spawnYMax - spawnYMin, 0f);
        Gizmos.DrawCube(center, size);
    }

    private void PrepopulateClouds()
    {
        foreach (var cb in _pool)
        {
            float y = Random.Range(spawnYMin, spawnYMax);
            float t = (y - spawnYMin) / (spawnYMax - spawnYMin);
            float scale = Mathf.Lerp(minScale, maxScale, t);
            float speed = Mathf.Lerp(minSpeed, maxSpeed, t) * globalSpeed;
            Color col   = Color.Lerp(backgroundColor, foregroundColor, t);

            float x = Random.Range(_leftEdge, _rightEdge);
            Vector3 start = new Vector3(x, y, 0f);
            float offscreenX = _rightEdge + spawnOffsetX;
            cb.Initialize(start, speed, scale, col, offscreenX);
        }
    }

    private IEnumerator SpawnLoop()
    {
        float wait = 1f / spawnRate;
        while (true)
        {
            SpawnOneCloud();
            yield return new WaitForSeconds(wait);
        }
    }

    private void SpawnOneCloud()
    {
        var cb = _pool.Find(c => !c.gameObject.activeInHierarchy);
        if (cb == null) return;

        float y = Random.Range(spawnYMin, spawnYMax);
        float t = (y - spawnYMin) / (spawnYMax - spawnYMin);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        float speed = Mathf.Lerp(minSpeed, maxSpeed, t) * globalSpeed;
        Color col   = Color.Lerp(backgroundColor, foregroundColor, t);

        Vector3 start = new Vector3(_leftEdge - spawnOffsetX, y, 0f);
        float offscreenX = _rightEdge + spawnOffsetX;
        cb.Initialize(start, speed, scale, col, offscreenX);
    }

    private class CloudBehavior : MonoBehaviour
    {
        private float _speed;
        private float _deactivateX;
        private SpriteRenderer _sr;

        public void Initialize(
            Vector3 startPos,
            float speed,
            float scale,
            Color color,
            float deactivateX
        ){
            transform.position    = startPos;
            transform.localScale  = Vector3.one * scale;
            _speed                = speed;
            _deactivateX          = deactivateX;

            if (_sr == null)
                _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null)
                _sr.color = color;

            gameObject.SetActive(true);
        }

        void Update()
        {
            transform.Translate(Vector3.right * _speed * Time.deltaTime);
            if (transform.position.x > _deactivateX)
                gameObject.SetActive(false);
        }
    }
}
