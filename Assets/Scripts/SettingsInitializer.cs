using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// On Awake, reads saved audio volumes from PlayerPrefs and applies them to the FMOD buses.
/// Attach this script to a GameObject that's always active at startup (e.g. an 'Initializer' object).
/// </summary>
public class SettingsInitializer : MonoBehaviour
{
    [Header("PlayerPrefs Keys")]
    [SerializeField]
    private string musicPrefKey = "MusicVolume";
    [SerializeField]
    private string sfxPrefKey   = "SFXVolume";

    [Header("Default Volumes (0–1)")]
    [Range(0f, 1f)]
    [SerializeField]
    private float defaultMusic = 0.75f;
    [Range(0f, 1f)]
    [SerializeField]
    private float defaultSFX   = 0.75f;

    [Header("FMOD Bus Paths")]
    [Tooltip("Path to your FMOD Music bus, e.g. 'bus:/Music' or 'bus:/Master/Music'")]
    [SerializeField]
    private string musicBusPath = "bus:/Music";
    [Tooltip("Path to your FMOD SFX bus, e.g. 'bus:/SFX' or 'bus:/Master/SFX'")]
    [SerializeField]
    private string sfxBusPath   = "bus:/SFX";

    void Awake()
    {
        // Load saved volumes (or fallback to defaults)
        float musicVol = PlayerPrefs.GetFloat(musicPrefKey, defaultMusic);
        float sfxVol   = PlayerPrefs.GetFloat(sfxPrefKey,   defaultSFX);

        // Apply to FMOD buses
        Bus musicBus = RuntimeManager.GetBus(musicBusPath);
        Bus sfxBus   = RuntimeManager.GetBus(sfxBusPath);
        musicBus.setVolume(musicVol);
        sfxBus.setVolume(sfxVol);
    }
}