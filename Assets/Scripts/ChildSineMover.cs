using System.Collections.Generic;
using UnityEngine;

public class ChildTransformSineMover : MonoBehaviour
{
    public enum MovementAxis { Vertical, Horizontal }

    [Header("Movement Settings")]
    [Tooltip("Which axis the children should move along.")]
    public MovementAxis axis = MovementAxis.Vertical;

    [Header("Wave Settings")]
    [Tooltip("Displacement in world units.")]
    public float amplitude = 1f;
    [Tooltip("Oscillations per second.")]
    public float frequency = 1f;
    [Tooltip("Phase offset per unit X when moving vertically.")]
    public float phaseOffsetPerX = 0.1f;
    [Tooltip("Phase offset per unit Y when moving horizontally.")]
    public float phaseOffsetPerY = 0.1f;

    private readonly Dictionary<Transform, Vector3> startPositions = new();
    private bool isRunning = true;

    private void OnEnable()
    {
        GameStateMachine.EnteringScoringAction += OnScoringStateEntered;
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringScoringAction -= OnScoringStateEntered;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        // Register any new immediate children and cache their first child’s transform position
        foreach (Transform child in transform)
        {
            if (child.childCount == 0)
                continue;

            Transform target = child.GetChild(0);

            if (!startPositions.ContainsKey(target))
                startPositions[target] = target.position;
        }

        float omega = frequency * 2f * Mathf.PI;
        float waveTime = Time.time * omega;

        var toRemove = new List<Transform>();

        foreach (var kv in startPositions)
        {
            Transform target = kv.Key;
            if (target == null)
            {
                toRemove.Add(target);
                continue;
            }

            Vector3 basePos = kv.Value;
            float phase, offset;

            if (axis == MovementAxis.Vertical)
            {
                phase = basePos.x * phaseOffsetPerX;
                offset = Mathf.Sin(waveTime + phase) * amplitude;
                target.position = new Vector3(basePos.x, basePos.y + offset, basePos.z);
            }
            else
            {
                phase = basePos.y * phaseOffsetPerY;
                offset = Mathf.Sin(waveTime + phase) * amplitude;
                target.position = new Vector3(basePos.x + offset, basePos.y, basePos.z);
            }
        }

        foreach (var t in toRemove)
            startPositions.Remove(t);
    }

    private void OnScoringStateEntered()
    {
        isRunning = false;
    }
}
