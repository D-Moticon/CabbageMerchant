using UnityEngine;
using System.Collections.Generic;

public class BoardMetrics : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridBounds = new Vector2(10f, 10f);
    public Vector2 gridSpacing = new Vector2(1f, 1f);

    private List<Vector2> gridPoints;

    private void GenerateGridPoints()
    {
        gridPoints = new List<Vector2>();

        Vector2 bottomLeft = (Vector2)transform.position - gridBounds / 2f;

        int cols = Mathf.FloorToInt(gridBounds.x / gridSpacing.x);
        int rows = Mathf.FloorToInt(gridBounds.y / gridSpacing.y);

        for (int x = 0; x <= cols; x++)
        {
            for (int y = 0; y <= rows; y++)
            {
                Vector2 point = bottomLeft + new Vector2(x * gridSpacing.x, y * gridSpacing.y);
                gridPoints.Add(point);
            }
        }
    }

    public Vector2[] GetRandomGridPoints(int amount)
    {
        if (gridPoints == null || gridPoints.Count == 0)
            GenerateGridPoints();

        List<Vector2> pointsCopy = new List<Vector2>(gridPoints);
        List<Vector2> randomPoints = new List<Vector2>();

        amount = Mathf.Min(amount, pointsCopy.Count);

        for (int i = 0; i < amount; i++)
        {
            int index = Random.Range(0, pointsCopy.Count);
            randomPoints.Add(pointsCopy[index]);
            pointsCopy.RemoveAt(index);
        }

        return randomPoints.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        GenerateGridPoints();

        Gizmos.color = Color.green;
        float sphereRadius = Mathf.Min(gridSpacing.x, gridSpacing.y) * 0.2f;

        foreach (Vector2 point in gridPoints)
        {
            Gizmos.DrawWireSphere(point, sphereRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, gridBounds);
    }
}