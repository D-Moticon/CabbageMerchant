using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class MusicManager : MonoBehaviour
{
    [Header("FMOD Music Event")]
    [Tooltip("Drag your looping music Event here (make sure it loops in FMOD Studio).")]
    [SerializeField]
    private EventReference musicEvent;

    private EventInstance musicInstance;

    private void Start()
    {
        PlayMusic();
    }

    /// <summary>
    /// Begins playing the assigned music event.
    /// If it’s already playing, it will first stop the old instance.
    /// </summary>
    public void PlayMusic()
    {
        StopCurrentInstance(STOP_MODE.IMMEDIATE);
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    /// <summary>
    /// Stops the music.  
    /// If immediate is false, allows FMOD’s fade‑out; otherwise cuts off instantly.
    /// </summary>
    public void StopMusic(bool immediate = false)
    {
        StopCurrentInstance(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
    }

    /// <summary>
    /// Swap to a new FMOD music event, optionally playing it immediately.
    /// </summary>
    /// <param name="newEvent">The EventReference you want to play next.</param>
    /// <param name="playNow">If true, starts playback immediately after swapping.</param>
    public void ChangeMusic(EventReference newEvent, bool playNow = true)
    {
        // stop whatever’s playing
        StopCurrentInstance(STOP_MODE.IMMEDIATE);

        // assign the new event
        musicEvent = newEvent;

        if (playNow)
            PlayMusic();
    }

    private void StopCurrentInstance(STOP_MODE mode)
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(mode);
            musicInstance.release();
            musicInstance.clearHandle();
        }
    }

    private void OnDestroy()
    {
        StopCurrentInstance(STOP_MODE.IMMEDIATE);
    }
}