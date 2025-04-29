using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class GrindRail : MonoBehaviour
{
    public EdgeCollider2D Edge { get; private set; }
    void Awake() => Edge = GetComponent<EdgeCollider2D>();

    /// Returns:
    /// • the closest point ON the rail to ‘worldPoint’
    /// • the unit tangent (direction along the rail) at that point
    public void GetClosestPointAndTangent(Vector2 worldPoint,
        out Vector2 closest,
        out Vector2 tangent)
    {
        var pts = Edge.points;
        closest = transform.position;
        tangent = Vector2.right;

        if (pts.Length < 2) return;

        float bestSqr = float.MaxValue;
        int   bestIdx = 0;

        for (int i = 0; i < pts.Length - 1; i++)
        {
            Vector2 a = transform.TransformPoint(pts[i]);
            Vector2 b = transform.TransformPoint(pts[i + 1]);

            Vector2 p = ClosestOnSegment(a, b, worldPoint);
            float   d = (worldPoint - p).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; bestIdx = i; closest = p; }
        }

        Vector2 s = transform.TransformPoint(pts[bestIdx]);
        Vector2 e = transform.TransformPoint(pts[bestIdx + 1]);
        tangent   = (e - s).normalized;
    }

    static Vector2 ClosestOnSegment(Vector2 A, Vector2 B, Vector2 P)
    {
        Vector2 AB = B - A;
        float t = Mathf.Clamp01(Vector2.Dot(P - A, AB) / AB.sqrMagnitude);
        return A + AB * t;
    }
}