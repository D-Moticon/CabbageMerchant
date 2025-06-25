using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CabbageSineMover : MonoBehaviour
{
    public enum MovementAxis { Vertical, Horizontal }

    [Header("Movement Settings")]
    [Tooltip("Which axis the cabbages should move along.")]
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

    // Tracks each cabbage Transform → its original start position
    private readonly Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
    private bool isRunning = true;

    private void OnEnable()
    {
        GameStateMachine.EnteringScoringAction += OnScoringStateEntered;
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringScoringAction -= OnScoringStateEntered;
    }

    void Update()
    {
        if (!isRunning)
            return;

        var active = GameSingleton.Instance.gameStateMachine.activeCabbages;

        // Register any new cabbages
        foreach (var cabbage in active)
        {
            if (cabbage == null) continue;
            if (cabbage.isDynamic) continue;
            Transform trs = cabbage.transform;
            if (!startPositions.ContainsKey(trs))
                startPositions[trs] = trs.position;
        }

        if (startPositions.Count == 0)
            return;

        // Precompute wave parameters
        float omega = frequency * 2f * Mathf.PI;
        float waveTime = Time.time * omega;

        var toRemove = new List<Transform>();
        foreach (var kv in startPositions)
        {
            Transform trs = kv.Key;
            if (trs == null)
            {
                toRemove.Add(trs);
                continue;
            }
            
            var cabbage = trs.GetComponent<Cabbage>();
            if (cabbage != null && cabbage.isDynamic)
                continue;

            Vector3 basePos = kv.Value;
            float phase, offset;

            if (axis == MovementAxis.Vertical)
            {
                phase = basePos.x * phaseOffsetPerX;
                offset = Mathf.Sin(waveTime + phase) * amplitude;
                trs.position = new Vector3(basePos.x, basePos.y + offset, basePos.z);
            }
            else // Horizontal
            {
                phase = basePos.y * phaseOffsetPerY;
                offset = Mathf.Sin(waveTime + phase) * amplitude;
                trs.position = new Vector3(basePos.x + offset, basePos.y, basePos.z);
            }
        }

        // Clean up destroyed cabbages
        foreach (var dead in toRemove)
            startPositions.Remove(dead);
    }

    void OnScoringStateEntered()
    {
        isRunning = false;
    }
}
