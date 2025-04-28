using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class FluidColliderVelocityTextureGenerator : MonoBehaviour
{
    public Shader drawShader;
    private Material drawMaterial;
    public RenderTexture externalVelocityRT;
    public float maxVelocity = 50f;
    private List<FluidCollider> fluidColliders = new List<FluidCollider>();

    void Start()
    {
        drawMaterial = new Material(drawShader);
        //forceTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        externalVelocityRT.enableRandomWrite = true;
    }

    public void AddToFluidColliders(FluidCollider fc)
    {
        fluidColliders.Add(fc);
    }

    public void RemoveFromFluidColliders(FluidCollider fc)
    {
        fluidColliders.Remove(fc);
    }

    void Update()
    {
        ClearRenderTexture(externalVelocityRT);

        // Find all FluidColliders
        //FluidCollider[] colliders = FindObjectsByType<FluidCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (FluidCollider collider in fluidColliders)
        {
            if (collider.enabled == false)
            {
                continue;
            }
            
            Vector2[] positions = collider.GetNormalizedCamPositions();
            Vector2[] velocities = collider.GetVelocities();
            //float radius = collider.GetRadius() * Camera.main.orthographicSize*2f;
            float radius = collider.GetRadius() * 20f;
            float mass = collider.GetMassMultiplier();

            if (positions == null || positions.Length == 0)
            {
                return;
            }
            
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 screenPos = positions[i].x * externalVelocityRT.width * Vector2.right + positions[i].y*externalVelocityRT.height*Vector2.up; // Convert to texture space
                Vector2 velocity = velocities[i] * mass; // Apply mass multiplier

                // Encode velocity in color
                float xCol = Helpers.RemapClamped(velocity.x, -maxVelocity, maxVelocity, 0f, 1f);
                float yCol = Helpers.RemapClamped(velocity.y, -maxVelocity, maxVelocity, 0f, 1f);
                Color velocityColor = new Color(xCol, yCol, 0);

                // Draw to the texture
                DrawPoint(screenPos, velocityColor, radius);
            }
        }
    }

    void DrawPoint(Vector2 pos, Color color, float radius)
    {
        drawMaterial.SetVector("_Point", new Vector4(pos.x, pos.y, radius, 0));
        drawMaterial.SetColor("_Color", color);
        drawMaterial.SetInt("_BlendMode", 1);
        Graphics.Blit(null, externalVelocityRT, drawMaterial);
    }

    void ClearRenderTexture(RenderTexture rt)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0, 0)); // Default to neutral force
        RenderTexture.active = active;
    }


    public RenderTexture GetForceTexture() => externalVelocityRT;
}
