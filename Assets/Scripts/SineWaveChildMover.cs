using UnityEngine;

[AddComponentMenu("Custom/Sine Wave Child Mover")]
public class SineWaveChildMover : MonoBehaviour
{
    [Tooltip("Amplitude of sine movement on X and Y axes.")]
    public Vector2 amplitude = Vector2.one;
    [Tooltip("Frequency of sine wave in degrees per second.")]
    public float frequency = 60f;
    [Tooltip("Phase difference between consecutive children in degrees.")]
    public float phaseDifference = 30f;

    private Transform[] children;
    private Vector3[] initialLocalPositions;

    void Start()
    {
        int count = transform.childCount;
        children = new Transform[count];
        initialLocalPositions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            children[i] = transform.GetChild(i);
            initialLocalPositions[i] = children[i].localPosition;
        }
    }

    void Update()
    {
        // Current time in seconds
        float t = Time.time;
        // Convert degrees/sec to radians/sec
        float omega = frequency * Mathf.Deg2Rad;
        // Convert phase difference to radians
        float phaseOffset = phaseDifference * Mathf.Deg2Rad;

        for (int i = 0; i < children.Length; i++)
        {
            // Compute total phase for this child
            float phase = t * omega + i * phaseOffset;
            float sinVal = Mathf.Sin(phase);
            // Calculate offset on each axis
            Vector3 offset = new Vector3(amplitude.x * sinVal, amplitude.y * sinVal, 0f);
            // Apply offset relative to initial position
            children[i].localPosition = initialLocalPositions[i] + offset;
        }
    }
}