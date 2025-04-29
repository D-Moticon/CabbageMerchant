using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CircleSpawner : MonoBehaviour
{
    [Tooltip("The prefab to spawn around the circle.")]
    public GameObject prefab;

    [Tooltip("How many copies to spawn.")]
    public int instanceCount = 8;

    [Tooltip("Radius of the circle (in local units).")]
    public float radius = 5f;

    [Tooltip("Starting angle for spawning, in degrees.")]
    public float startAngle = 0f;

    [Tooltip("Ending angle for spawning, in degrees.")]
    public float endAngle = 360f;

    [Tooltip("Additional rotation offset for each spawned piece, in degrees.")]
    public float pieceRotationOffset = 0f;

    [Tooltip("If true, each instance is rotated so its forward (up) faces outward plus the piece offset.")]
    public bool alignToCircle = true;

    [Tooltip("Radius of the gizmo spheres drawn at each instance position.")]
    public float gizmoSphereRadius = 0.25f;

    // Tracks all instances spawned by this spawner
    private List<GameObject> spawnedInstances = new List<GameObject>();

    [Button("Spawn Circle")]
    private void SpawnCircle()
    {
        // Cleanup previously spawned instances
        for (int i = spawnedInstances.Count - 1; i >= 0; i--)
        {
            var go = spawnedInstances[i];
            if (go != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(go);
#else
                Destroy(go);
#endif
            }
        }
        spawnedInstances.Clear();

        if (prefab == null)
        {
            Debug.LogWarning("CircleSpawner: prefab is null!");
            return;
        }
        if (instanceCount <= 0)
        {
            Debug.LogWarning("CircleSpawner: instanceCount must be > 0!");
            return;
        }

        float arc = endAngle - startAngle;
        float angleStep = (instanceCount > 1) ? (arc / (instanceCount - 1)) : 0f;

        for (int i = 0; i < instanceCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 localPos = new Vector3(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius,
                0f
            );

            GameObject go = GameObject.Instantiate(prefab, transform);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one;

            // Apply rotation with optional alignment and additional offset
            float rotationZ = alignToCircle ? (angle + pieceRotationOffset) : pieceRotationOffset;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

            spawnedInstances.Add(go);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw base circle
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);

        if (prefab == null || instanceCount <= 0)
            return;

        float arc = endAngle - startAngle;
        float angleStep = (instanceCount > 1) ? (arc / (instanceCount - 1)) : 0f;

        for (int i = 0; i < instanceCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 localPos = new Vector3(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius,
                0f
            );
            Vector3 worldPos = transform.TransformPoint(localPos);

            Gizmos.DrawWireSphere(worldPos, gizmoSphereRadius);
        }
    }
}
