using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderNumber : MonoBehaviour
{
    [Tooltip("The UI Slider whose value to display.")]
    public Slider slider;

    [Tooltip("The Text (UI.Text or TMP_Text) to update with the slider's value.")]
    public Text uiText;                   // for legacy UI.Text
    public TMP_Text tmpText;              // for TextMeshProUGUI

    [Tooltip("Numeric format string (e.g. “F0”, “F2”, “0.00”).")]
    public string format = "F0";

    void Reset()
    {
        // Try to auto-assign the slider on the same GameObject
        slider = GetComponent<Slider>();
    }

    void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        // Subscribe to value changes
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnDestroy()
    {
        // Clean up subscription
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    void Start()
    {
        // Initialize display
        UpdateText(slider.value);
    }

    private void OnSliderValueChanged(float newValue)
    {
        UpdateText(newValue);
    }

    private void UpdateText(float v)
    {
        string s = v.ToString(format);
        if (uiText != null)
            uiText.text = s;
        if (tmpText != null)
            tmpText.text = s;
    }
}