using UnityEngine;

/// <summary>
/// Applies a sine-wave offset to the GameObject's transform each frame.
/// You can configure separate amplitude, frequency (in degrees per second),
/// and initial phase offset for X and Y axes.
/// </summary>
[RequireComponent(typeof(Transform))]
public class SineWaveMover : MonoBehaviour
{
    [Header("Amplitude (in units)")]
    [Tooltip("Peak displacement on X axis")]
    public float amplitudeX = 1f;
    [Tooltip("Peak displacement on Y axis")]
    public float amplitudeY = 1f;

    [Header("Frequency (degrees per second)")]
    [Tooltip("Oscillation speed on X axis in degrees/sec")]
    public float frequencyX = 45f;
    [Tooltip("Oscillation speed on Y axis in degrees/sec")]
    public float frequencyY = 45f;

    [Header("Phase Offset (degrees)")]
    [Tooltip("Starting phase on X axis in degrees")]
    public float phaseOffsetX = 0f;
    [Tooltip("Starting phase on Y axis in degrees")]
    public float phaseOffsetY = 90f;

    // original local position
    private Vector3 startPos;

    void Awake()
    {
        // record starting position
        startPos = transform.localPosition;
    }

    void Update()
    {
        // time-based angle in degrees
        float angleX = frequencyX * Time.time + phaseOffsetX;
        float angleY = frequencyY * Time.time + phaseOffsetY;

        // convert to radians for Mathf.Sin
        float radX = angleX * Mathf.Deg2Rad;
        float radY = angleY * Mathf.Deg2Rad;

        // compute offsets
        float offsetX = amplitudeX * Mathf.Sin(radX);
        float offsetY = amplitudeY * Mathf.Sin(radY);

        // apply
        transform.localPosition = startPos + new Vector3(offsetX, offsetY, 0f);
    }
}