using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderFlashOnThreshold : MonoBehaviour
{
    [Header("Threshold Settings")]
    [Tooltip("Start flashing when slider.value is below this.")]
    public float threshold = 0.25f;

    [Header("Flash Colors")]
    [Tooltip("Color at minimum brightness.")]
    public Color darkColor = Color.red;
    [Tooltip("Color at maximum brightness.")]
    public Color lightColor = Color.white;

    [Header("Oscillation")]
    [Tooltip("Speed of the flash oscillation.")]
    public float oscillationSpeed = 2f;

    // references
    private Slider slider;
    private Image fillImage;
    private Color originalColor;

    void Awake()
    {
        slider = GetComponent<Slider>();

        // grab the fill Image; Slider.fillRect must be set in the Slider inspector
        if (slider.fillRect == null)
        {
            Debug.LogError("SliderFlashOnThreshold: Slider.fillRect is not assigned!", this);
            enabled = false;
            return;
        }

        fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage == null)
        {
            Debug.LogError("SliderFlashOnThreshold: No Image component found on fillRect!", this);
            enabled = false;
            return;
        }

        // remember original so we can restore when not flashing
        originalColor = fillImage.color;
    }

    void Update()
    {
        if (slider.value <= threshold)
        {
            // ping-pong between 0 and 1
            float t = Mathf.PingPong(Time.time * oscillationSpeed, 1f);
            fillImage.color = Color.Lerp(darkColor, lightColor, t);
        }
        else
        {
            // restore the normal color
            fillImage.color = originalColor;
        }
    }
}