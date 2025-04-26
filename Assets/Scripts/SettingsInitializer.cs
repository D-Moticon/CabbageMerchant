using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// On Awake, reads saved settings from PlayerPrefs (audio, VFX toggles, FPS, resolution, fullscreen)
/// and applies them to FMOD, the EffectsManager, and Unity’s Application/Screen settings.
/// Attach this script to a GameObject that’s always active at startup.
/// </summary>
public class SettingsInitializer : MonoBehaviour
{
    [Header("PlayerPrefs Keys")]
    [SerializeField] private string musicPrefKey       = "MusicVolume";
    [SerializeField] private string sfxPrefKey         = "SFXVolume";
    [SerializeField] private string fluidVFXKey        = "Setting_FluidVFX";
    [SerializeField] private string fpsKey             = "Setting_FPS";
    [SerializeField] private string resolutionIndexKey = "Setting_ResolutionIndex";
    [SerializeField] private string fullscreenKey      = "Setting_Fullscreen";

    [Header("Default Volumes (0–1)")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusic = 0.75f;
    [Range(0f, 1f)]
    [SerializeField] private float defaultSFX   = 1.00f;

    [Header("FMOD Bus Paths")]
    [Tooltip("Path to your FMOD Music bus, e.g. 'bus:/Music'")]
    [SerializeField] private string musicBusPath = "bus:/Music";
    [Tooltip("Path to your FMOD SFX bus, e.g. 'bus:/SFX'")]
    [SerializeField] private string sfxBusPath   = "bus:/SFX";

    void Awake()
    {
        // --- Audio ---
        float musicVol = PlayerPrefs.GetFloat(musicPrefKey, defaultMusic);
        float sfxVol   = PlayerPrefs.GetFloat(sfxPrefKey,   defaultSFX);

        Bus musicBus = RuntimeManager.GetBus(musicBusPath);
        Bus sfxBus   = RuntimeManager.GetBus(sfxBusPath);
        musicBus.setVolume(musicVol);
        sfxBus.setVolume(sfxVol);

        // --- Fluid VFX toggle ---
        bool fluidOn = PlayerPrefs.GetInt(fluidVFXKey, 1) == 1;
        Singleton.Instance.effectsManager.SetFluidVFX(fluidOn);

        // --- FPS cap ---
        int fps = PlayerPrefs.GetInt(fpsKey, 160);
        Application.targetFrameRate = fps;

        // --- Fullscreen mode (default = true) ---
        bool isFull = PlayerPrefs.GetInt(fullscreenKey, 1) == 1;

        // --- Resolution (default = 1920×1080) ---
        Resolution[] resolutions = Screen.resolutions;
        // find our default
        int defaultIndex = System.Array.FindIndex(resolutions, r =>
            r.width == 1920 && r.height == 1080);
        if (defaultIndex < 0)
            defaultIndex = resolutions.Length - 1; // fallback to highest
        int savedIndex = PlayerPrefs.GetInt(resolutionIndexKey, defaultIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, resolutions.Length - 1);
        var res = resolutions[savedIndex];

        // apply fullscreen & resolution
        Screen.SetResolution(res.width, res.height, isFull);
    }
}
