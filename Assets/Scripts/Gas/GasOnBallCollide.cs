using System;
using UnityEngine;

public class GasOnBallCollide : MonoBehaviour
{
    public float forceVel = 50f;
    public float forceRad = 1f;
    public float densityInjection = 1f;
    [ColorUsage(true, true)] public Color color;
    public float temperature = 0f;
    public float spawnPointOffset = 0.5f;
    
    [Header("Cabbage Merge")]
    public float densityInjection_Cabbage = 10f;
    [ColorUsage(true, true)] public Color color_Cabbage;
    public int cabbageMergeSpawnPoints = 8;
    
    private void OnEnable()
    {
        Ball.BallCollidedEvent += BallCollidedListener;
        Cabbage.CabbageMergedEvent += CabbageMergedListener;
    }

    private void OnDisable()
    {
        Ball.BallCollidedEvent -= BallCollidedListener;
        Cabbage.CabbageMergedEvent -= CabbageMergedListener;
    }

    void BallCollidedListener(Ball b, Collision2D col)
    {
        Vector2 dir = col.contacts[0].normal;
        Vector2 pos = col.contacts[0].point + dir*spawnPointOffset;
        Vector2 vel = forceVel * dir * col.relativeVelocity.magnitude;
        
        GameSingleton.Instance.gasSim.SpawnGasAtPosition(pos, vel, forceRad, densityInjection, color, temperature);
    }

    void CabbageMergedListener(Cabbage.CabbageMergedParams cmp)
    {
        float rad = 6f;

        for (int i = 0; i < cabbageMergeSpawnPoints; i++)
        {
            float ang = 360f / cabbageMergeSpawnPoints * i;
            Vector2 dir = Helpers.AngleDegToVector2(ang);
            Vector2 pos = cmp.pos + cmp.newCabbage.transform.lossyScale.x * 0.52f * dir;
            Vector2 vel = dir * 30f;
            GameSingleton.Instance.gasSim.SpawnGasAtPosition(pos, vel, rad, densityInjection_Cabbage, color_Cabbage, temperature);
        }
        
    }
}
