using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabbageCircleMover : MonoBehaviour
{
    [Header("Circle Settings")]
    [Tooltip("Angular speed in degrees per second.")]
    [SerializeField] private float angularSpeed = 360f;

    private struct OrbitData
    {
        public float StartAngle;
        public float Radius;
        public float RegisteredTime;
    }

    private readonly Dictionary<Transform, OrbitData> orbitData = new Dictionary<Transform, OrbitData>();
    private bool isRunning = false;

    private void OnEnable()
    {
        GameStateMachine.BoardFinishedPopulatingAction += OnBoardPopulated;
        GameStateMachine.EnteringScoringAction += OnScoringStateEntered;
        Cabbage.CabbageMergedEvent += OnCabbageMerged;
    }

    private void OnDisable()
    {
        GameStateMachine.BoardFinishedPopulatingAction -= OnBoardPopulated;
        GameStateMachine.EnteringScoringAction -= OnScoringStateEntered;
        Cabbage.CabbageMergedEvent -= OnCabbageMerged;
    }

    private void OnBoardPopulated()
    {
        StartCoroutine(StartCircleCoroutine());
    }

    IEnumerator StartCircleCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        isRunning = true;
        orbitData.Clear();

        foreach (var cabbage in GameSingleton.Instance.gameStateMachine.activeCabbages)
            RegisterCabbage(cabbage.transform);
    }

    private void Update()
    {
        if (!isRunning)
            return;

        float angularSpeedRad = angularSpeed * Mathf.Deg2Rad;
        Vector3 center = transform.position;

        foreach (var kv in orbitData)
        {
            Transform trs = kv.Key;
            if (trs == null) continue;

            OrbitData data = kv.Value;
            float elapsedTime = Time.time - data.RegisteredTime;
            float currentAngle = data.StartAngle + elapsedTime * angularSpeedRad;

            float x = Mathf.Cos(currentAngle) * data.Radius;
            float y = Mathf.Sin(currentAngle) * data.Radius;

            trs.position = new Vector3(center.x + x, center.y + y, trs.position.z);
        }
    }

    private void OnScoringStateEntered()
    {
        isRunning = false;
    }

    private void OnCabbageMerged(Cabbage.CabbageMergedParams mergeParams)
    {
        // Remove potentially outdated orbit data
        orbitData.Remove(mergeParams.newCabbage.transform);
        
        // Immediately register with the exact merge position, correctly computing its start angle
        RegisterCabbage(mergeParams.newCabbage.transform, mergeParams.pos);
    }

    // Default registration at current transform position
    private void RegisterCabbage(Transform trs)
    {
        RegisterCabbage(trs, trs.position);
    }

    // Overload allows explicitly registering at a known correct position
    private void RegisterCabbage(Transform trs, Vector3 position)
    {
        Vector3 offset = position - transform.position;
        float angle = Mathf.Atan2(offset.y, offset.x);
        float radius = offset.magnitude;

        orbitData[trs] = new OrbitData
        {
            StartAngle = angle,
            Radius = radius,
            RegisteredTime = Time.time
        };
    }

}
