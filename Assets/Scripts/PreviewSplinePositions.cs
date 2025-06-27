using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Draws gizmo previews for evenly spaced "squares" along a SplineContainer in the Scene view.
/// Attach this to the same GameObject as your SplineContainer (or reference one).
/// </summary>
[ExecuteAlways]
public class PreviewSplinePositions : MonoBehaviour
{
    [Header("Spline Reference")]
    [Tooltip("The SplineContainer whose spline will be sampled.")]
    public SplineContainer splineContainer;

    [Header("Preview Settings")]
    [Min(1)]
    [Tooltip("How many preview squares to draw along the spline.")]
    public int numberSquares = 10;

    [Tooltip("Size of each preview square in local units.")]
    public Vector3 squareSize = Vector3.one;

    [Tooltip("Color of the wireframe squares.")]
    public Color gizmoColor = Color.green;

    [Tooltip("If true, each square will be rotated to align with the spline tangent.")]
    public bool orientToTangent = false;

    void OnDrawGizmos()
    {
        if (splineContainer == null || numberSquares < 1)
            return;

        Gizmos.color = gizmoColor;

        // Prevent division by zero when only one square
        float denom = Mathf.Max(1, numberSquares - 1);

        for (int i = 0; i < numberSquares; i++)
        {
            float t = i / denom;
            Vector3 pos = splineContainer.EvaluatePosition(t);
            Quaternion rot = Quaternion.identity;

            if (orientToTangent)
            {
                Vector3 tangent = splineContainer.EvaluateTangent(t);
                float angleZ = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                rot = Quaternion.Euler(0f, 0f, angleZ);
            }

            // Construct a matrix for position, rotation, and scale
            Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, squareSize);
            Gizmos.matrix = matrix;

            // Draw a unit cube, which the matrix transforms into a square
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        // Reset Gizmos matrix so it doesn't affect other gizmos
        Gizmos.matrix = Matrix4x4.identity;
    }
}
