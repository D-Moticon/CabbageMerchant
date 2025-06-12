using System;
using UnityEngine;
using System.Collections.Generic;

public class FluidCollider : MonoBehaviour
{
    public float colliderDetail = 1; // Number of sample points
    public float pointRadius = 0.1f; // Radius of each point
    public float massMultiplier = 1.0f; // Mass influence on fluid interaction

    Rigidbody2D rb;
    private Collider2D col;
    private List<Transform> samplePoints = new List<Transform>();
    private Vector2[] previousWorldPositions;
    private Vector2[] normalizedCamPositions;
    private Vector2[] velocities;

    private void OnEnable()
    {
        GameStateMachine.GSM_Enabled_Event += GameStateMachineEnabledListener;
        
        if (GameSingleton.Instance == null)
        {
            return;
        }
        GameSingleton.Instance.fluidRTReferences.fluidColliderVelocityTextureGenerator.AddToFluidColliders(this);
    }

    private void OnDisable()
    {
        GameStateMachine.GSM_Enabled_Event -= GameStateMachineEnabledListener;
        if (GameSingleton.Instance == null)
        {
            return;
        }
        GameSingleton.Instance.fluidRTReferences.fluidColliderVelocityTextureGenerator.RemoveFromFluidColliders(this);
    }
    
    private void GameStateMachineEnabledListener(GameStateMachine gsm)
    {
        if (GameSingleton.Instance == null)
        {
            print("NU");
            return;
        }
        
        GameSingleton.Instance.fluidRTReferences.fluidColliderVelocityTextureGenerator.AddToFluidColliders(this);
    }

    void Start()
    {
        col = GetComponent<Collider2D>();

        // Generate sample points along the collider
        samplePoints = GenerateSamplePoints();

        previousWorldPositions = new Vector2[samplePoints.Count];
        for (int i = 0; i < samplePoints.Count; i++)
        {
            previousWorldPositions[i] = samplePoints[i].position;
        }

        normalizedCamPositions = new Vector2[samplePoints.Count];
        velocities = new Vector2[samplePoints.Count];

        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            UpdateSamplePoints(Time.fixedDeltaTime);
        }
    }

    private void Update()
    {
        if (rb == null)
        {
            UpdateSamplePoints(Time.deltaTime);
        }
    }

    void UpdateSamplePoints(float timeStep)
    {
        if (GameSingleton.Instance == null)
        {
            return;
        }
        
        RenderTexture renderTexture = GameSingleton.Instance.fluidRTReferences.externalVelocityRT;
        float halfHeight = 10f;
        float halfWidth = halfHeight * 16f / 9f;

        if (Camera.main == null)
        {
            return;
        }
        
        for (int i = 0; i < samplePoints.Count; i++)
        {
            Vector2 vel = ((Vector2)samplePoints[i].position - previousWorldPositions[i]) / timeStep;
            velocities[i] = vel;
            previousWorldPositions[i] = samplePoints[i].position;

            //normalizedCamPositions[i] = Camera.main.WorldToViewportPoint(samplePoints[i].position);
            
            /*// Convert world position to camera-local space.
            // This makes the camera's center (and its orientation) our reference.
            Vector3 localPos = Camera.main.transform.InverseTransformPoint(samplePoints[i].position);
        
            // Normalize based on our virtual extents.
            // localPos.x should be between -halfWidth and halfWidth,
            // localPos.y should be between -halfHeight and halfHeight.
            float normalizedX = (localPos.x / (2f * halfWidth)) + 0.5f;
            float normalizedY = (localPos.y / (2f * halfHeight)) + 0.5f;
        
            normalizedCamPositions[i] = new Vector2(normalizedX, normalizedY);*/
            
            Vector3 vp = Camera.main.WorldToViewportPoint(samplePoints[i].position);
            normalizedCamPositions[i] = new Vector2(vp.x, vp.y);
            normalizedCamPositions[i].x = Mathf.Clamp01(normalizedCamPositions[i].x);
            normalizedCamPositions[i].y = Mathf.Clamp01(normalizedCamPositions[i].y);
        }
    }

    List<Transform> GenerateSamplePoints()
    {
        if (col is BoxCollider2D)
        {
            return GenerateBoxSamplePoints(col as BoxCollider2D);
        }

        return GenerateCircleSamplePoints(col as CircleCollider2D);
        
    }

    List<Transform> GenerateCircleSamplePoints(CircleCollider2D collider2D)
    {
        List<Transform> points = new List<Transform>();
        
        GameObject pointObj = new GameObject("FluidPoint");
        pointObj.transform.SetParent(transform);
        Vector2 pos = Vector2.zero;
        Quaternion rot = Quaternion.identity;

        pointObj.transform.localPosition = pos;
        pointObj.transform.localRotation = rot;

        points.Add(pointObj.transform);

        return points;
    }

    List<Transform> GenerateBoxSamplePoints(BoxCollider2D bc2d)
    {
        int colDetail = Mathf.RoundToInt(colliderDetail);

        List<Transform> samplePoints = new List<Transform>();

        Vector2 size = bc2d.size;
        Vector2 offset = bc2d.offset; // Local space offset

        // Define the four corners in local space
        Vector2[] corners = new Vector2[]
        {
        new Vector2(-size.x / 2, -size.y / 2) + offset, // Bottom-left
        new Vector2(size.x / 2, -size.y / 2) + offset,  // Bottom-right
        new Vector2(size.x / 2, size.y / 2) + offset,   // Top-right
        new Vector2(-size.x / 2, size.y / 2) + offset   // Top-left
        };

        // Convert corners to world space
        for (int i = 0; i < 4; i++)
        {
            corners[i] = bc2d.transform.TransformPoint(corners[i]);
        }

        // Iterate through each edge
        for (int i = 0; i < 4; i++)
        {
            Vector2 start = corners[i];
            Vector2 end = corners[(i + 1) % 4]; // Wrap around to connect edges

            // Add the corner point
            samplePoints.Add(CreateSamplePoint(start, this.transform));

            // Distribute `colDetail` points along this edge
            for (int j = 1; j <= colDetail; j++)
            {
                float t = (float)j / (colDetail + 1); // Avoid placing on corners again
                Vector2 newPoint = Vector2.Lerp(start, end, t);
                samplePoints.Add(CreateSamplePoint(newPoint, this.transform));
            }
        }

        return samplePoints;
    }

    /// <summary>
    /// Creates a sample point transform at a given position.
    /// </summary>
    Transform CreateSamplePoint(Vector2 position, Transform parent)
    {
        GameObject pointObj = new GameObject("FluidPoint");
        pointObj.transform.position = position;
        pointObj.transform.SetParent(parent);
        return pointObj.transform;
    }


    public Vector2[] GetNormalizedCamPositions() => normalizedCamPositions;
    public Vector2[] GetVelocities() => velocities;
    public float GetRadius() => pointRadius;
    public float GetMassMultiplier() => massMultiplier;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, pointRadius);
    }
    
    
}
