using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class SettingsPanel : MenuPanel
{
    [Header("Audio Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;
    [Tooltip("FMOD bus path for music")] 
    public string musicBusPath = "bus:/Music";
    [Tooltip("FMOD bus path for SFX")] 
    public string sfxBusPath   = "bus:/SFX";

    [Header("Graphics & Gameplay")]
    public Toggle fluidVFXToggle;
    public Slider fpsSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    // Pref keys & defaults
    const string MusicKey       = "MusicVolume";
    const string SfxKey         = "SFXVolume";
    const string FluidVFXKey    = "Setting_FluidVFX";
    const string FPSKey         = "Setting_FPS";
    const string ResolutionKey  = "Setting_ResolutionIndex";
    const string FullscreenKey  = "Setting_Fullscreen";

    const float  DefaultMusic   = 0.75f;
    const float  DefaultSfx     = 0.75f;

    // runtime caches
    private Bus musicBus;
    private Bus sfxBus;
    private List<Resolution> availableResolutions;

    void Awake()
    {
        // cache FMOD buses
        musicBus = RuntimeManager.GetBus(musicBusPath);
        sfxBus   = RuntimeManager.GetBus(sfxBusPath);

        // build resolution list
        availableResolutions = new List<Resolution>(Screen.resolutions);
        var options = new List<string>(availableResolutions.Count);
        foreach (var res in availableResolutions)
            options.Add($"{res.width} x {res.height}");
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    public override void OnShow()
    {
        base.OnShow();

        // --- Load & apply prefs ---
        // Audio
        float m = PlayerPrefs.GetFloat(MusicKey, DefaultMusic);
        float s = PlayerPrefs.GetFloat(SfxKey,   DefaultSfx);
        musicSlider.value = m;
        sfxSlider.value   = s;
        musicBus.setVolume(m);
        sfxBus.setVolume(s);

        // Fluid VFX
        bool fluidOn = PlayerPrefs.GetInt(FluidVFXKey, 1) == 1;
        fluidVFXToggle.isOn = fluidOn;
        Singleton.Instance.effectsManager.SetFluidVFX(fluidOn);

        // FPS
        int savedFPS = PlayerPrefs.GetInt(FPSKey, 60);
        fpsSlider.value = Mathf.Clamp(savedFPS, (int)fpsSlider.minValue, (int)fpsSlider.maxValue);
        Application.targetFrameRate = savedFPS;

        // Resolution
        int savedResIdx = PlayerPrefs.GetInt(ResolutionKey, availableResolutions.Count - 1);
        savedResIdx = Mathf.Clamp(savedResIdx, 0, availableResolutions.Count - 1);
        resolutionDropdown.value = savedResIdx;
        var res = availableResolutions[savedResIdx];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);

        // Fullscreen
        bool isFull = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.isOn = isFull;
        Screen.fullScreen = isFull;

        // --- Hook callbacks ---
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        fluidVFXToggle.onValueChanged.AddListener(OnFluidVFXChanged);
        fpsSlider.onValueChanged.AddListener(OnFpsChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        // focus first control
        musicSlider.Select();
    }

    public override void OnHide()
    {
        // --- Unhook callbacks ---
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        fluidVFXToggle.onValueChanged.RemoveListener(OnFluidVFXChanged);
        fpsSlider.onValueChanged.RemoveListener(OnFpsChanged);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);

        base.OnHide();
    }

    //---- Callbacks ----

    private void OnMusicChanged(float v)
    {
        musicBus.setVolume(v);
        PlayerPrefs.SetFloat(MusicKey, v);
        PlayerPrefs.Save();
    }

    private void OnSfxChanged(float v)
    {
        sfxBus.setVolume(v);
        PlayerPrefs.SetFloat(SfxKey, v);
        PlayerPrefs.Save();
    }

    private void OnFluidVFXChanged(bool isOn)
    {
        Singleton.Instance.effectsManager.SetFluidVFX(isOn);
        PlayerPrefs.SetInt(FluidVFXKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnFpsChanged(float v)
    {
        int fps = Mathf.RoundToInt(v);
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(FPSKey, fps);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int idx)
    {
        idx = Mathf.Clamp(idx, 0, availableResolutions.Count - 1);
        var r = availableResolutions[idx];
        Screen.SetResolution(r.width, r.height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt(ResolutionKey, idx);
        PlayerPrefs.Save();
    }

    private void OnFullscreenChanged(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt(FullscreenKey, isFull ? 1 : 0);
        PlayerPrefs.Save();
    }
}
