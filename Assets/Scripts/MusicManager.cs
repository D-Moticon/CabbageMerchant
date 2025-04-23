using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public EventReference startingMusic;
    
    [System.Serializable]
    public class BiomeMusicPair
    {
        public Biome biome;
        public EventReference music;
    }

    public List<BiomeMusicPair> biomeMusicPairs;
    private EventReference musicEvent;
    private EventInstance musicInstance;

    private void OnEnable()
    {
        RunManager.BiomeChangedEvent += BiomeChangedListener;
        ChangeMusic(startingMusic);
    }

    private void OnDisable()
    {
        RunManager.BiomeChangedEvent -= BiomeChangedListener;
    }

    void PlayMusic()
    {
        StopCurrentInstance(STOP_MODE.IMMEDIATE);
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }
    
    void BiomeChangedListener(Biome newBiome)
    {
        for (int i = 0; i < biomeMusicPairs.Count; i++)
        {
            if (biomeMusicPairs[i].biome == newBiome)
            {
                ChangeMusic(biomeMusicPairs[i].music);
            }
        }
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