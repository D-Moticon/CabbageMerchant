using System;
using UnityEngine;
using Sirenix.OdinInspector;
using MoreMountains.Feedbacks;

public class BallBooper : MonoBehaviour
{
    [Header("Booper Settings")]
    public Vector2 speedRange = new Vector2(5f, 10f);
    public float spreadAngle = 45f;

    public enum AngleMode
    {
        normal,          // Use collision normal
        fixedDirection   // Use the specified angle
    }

    [EnumToggleButtons]
    public AngleMode angleMode;

    [ShowIf("@angleMode == AngleMode.fixedDirection")]
    [LabelText("Fixed Angle (degrees)")]
    public float fixedDirectionAngle;

    public MMF_Player boopFeel;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Ball ball = other.gameObject.GetComponent<Ball>();
        if (ball == null) return;

        // Base angle depends on mode:
        // - normal: collision normal
        // - fixedDirection: fixedDirectionAngle
        float baseAngle;
        if (angleMode == AngleMode.normal)
        {
            // Use the first contact's normal as the base angle
            Vector2 normal = other.contacts[0].normal;
            baseAngle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
        }
        else // fixedDirection
        {
            baseAngle = fixedDirectionAngle;
        }

        // Generate a random offset within [-spreadAngle/2..spreadAngle/2]
        float offset = UnityEngine.Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);

        // Final angle
        float finalAngle = baseAngle + offset;

        // Convert angle to a direction (in degrees)
        Vector2 direction = AngleToVector2(finalAngle);

        // Random speed from speedRange
        float speed = UnityEngine.Random.Range(speedRange.x, speedRange.y);

        // Apply velocity to the ball
        ball.rb.linearVelocity = direction * speed;

        if (boopFeel != null)
        {
            boopFeel.PlayFeedbacks();
        }
    }

    /// <summary>
    /// Converts an angle in degrees to a 2D unit vector.
    /// 0 degrees is right (1,0); 90 degrees is up (0,1).
    /// </summary>
    private Vector2 AngleToVector2(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
