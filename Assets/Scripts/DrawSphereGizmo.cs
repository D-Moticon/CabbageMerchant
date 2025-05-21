using System;
using UnityEngine;

public class DrawSphereGizmo : MonoBehaviour
{
    public float radius;
    public Color color = Color.white;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(this.transform.position,radius);
    }
}
