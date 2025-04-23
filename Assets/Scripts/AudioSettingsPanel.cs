using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class AudioSettingsPanel : MenuPanel
{
    [Header("UI References")]
    [Tooltip("Slider for the music volume (0–1)")]
    public Slider musicSlider;
    [Tooltip("Slider for the SFX volume (0–1)")]
    public Slider sfxSlider;

    [Header("FMOD Buses")]
    [Tooltip("FMOD bus path for music (e.g. \"bus:/Music\")")]
    public string musicBusPath = "bus:/Music";
    [Tooltip("FMOD bus path for SFX (e.g. \"bus:/SFX\")")]
    public string sfxBusPath   = "bus:/SFX";

    const string MusicPrefKey = "MusicVolume";
    const string SFXPrefKey   = "SFXVolume";
    const float  DefaultMusic = 0.75f;
    const float  DefaultSFX   = 0.75f;

    Bus musicBus;
    Bus sfxBus;

    void Awake()
    {
        // cache the FMOD buses
        musicBus = RuntimeManager.GetBus(musicBusPath);
        sfxBus   = RuntimeManager.GetBus(sfxBusPath);
    }

    void Start()
    {
        // load saved volumes (or defaults) and apply
        float m = PlayerPrefs.GetFloat(MusicPrefKey, DefaultMusic);
        float s = PlayerPrefs.GetFloat(SFXPrefKey,   DefaultSFX);

        musicSlider.value = m;
        sfxSlider.value   = s;

        SetMusicVolume(m);
        SetSFXVolume(s);

        // hook slider callbacks
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    void OnMusicChanged(float v)
    {
        SetMusicVolume(v);
        PlayerPrefs.SetFloat(MusicPrefKey, v);
        PlayerPrefs.Save();
    }

    void OnSFXChanged(float v)
    {
        SetSFXVolume(v);
        PlayerPrefs.SetFloat(SFXPrefKey, v);
        PlayerPrefs.Save();
    }

    void SetMusicVolume(float v)
    {
        // FMOD accepts a 0–1 linear volume
        musicBus.setVolume(v);
    }

    void SetSFXVolume(float v)
    {
        sfxBus.setVolume(v);
    }

    public override void OnShow()
    {
        base.OnShow();
        // re‑sync sliders in case prefs changed elsewhere
        musicSlider.value = PlayerPrefs.GetFloat(MusicPrefKey, DefaultMusic);
        sfxSlider.value   = PlayerPrefs.GetFloat(SFXPrefKey,   DefaultSFX);
        musicSlider.Select();
    }
}
