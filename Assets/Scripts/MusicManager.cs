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
    public EventReference overworldMusic;
    public EventReference victoryMusic;
    private EventReference musicEvent;
    private EventInstance musicInstance;
    private int currentMusicPhase = 0;

    private void OnEnable()
    {
        RunManager.BiomeChangedEvent += BiomeChangedListener;
        RunManager.RunFinishedEvent += RunFinishedListener;
        RunManager.RunStartEvent += RunStartedListener;
        OverworldManager.overworldStartedAction += OverworldStartedListener;
        ChangeMusic(startingMusic);
    }

    private void OnDisable()
    {
        RunManager.BiomeChangedEvent -= BiomeChangedListener;
        RunManager.RunFinishedEvent -= RunFinishedListener;
        RunManager.RunStartEvent -= RunStartedListener;
        OverworldManager.overworldStartedAction -= OverworldStartedListener;
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
    public void ChangeMusic(EventReference newEvent, bool playNow = true, bool forceRestart = false)
    {
        if (!forceRestart)
        {
            if (musicInstance.isValid() && newEvent.Guid == musicEvent.Guid)
            {
                // already playing that exact event → nothing to do
                return;
            }
        }

        SetMusicPhase(0);
        
        // stop whatever’s playing
        StopCurrentInstance(STOP_MODE.IMMEDIATE);

        // assign the new event
        musicEvent = newEvent;

        if (playNow)
            PlayMusic();
    }

    public int GetMusicPhase()
    {
        return currentMusicPhase;
    }
    
    public void SetMusicPhase(int newMusicPhase)
    {
        currentMusicPhase = newMusicPhase;
        musicInstance.setParameterByName("region", currentMusicPhase);
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

    void RunFinishedListener(RunManager.RunCompleteParams rcp)
    {
        if (rcp.success)
        {
            ChangeMusic(victoryMusic);
        }
    }

    void OverworldStartedListener()
    {
        ChangeMusic(overworldMusic);
    }

    void RunStartedListener(RunManager.RunStartParams rsp)
    {
        ChangeMusic(biomeMusicPairs[0].music);
    }
}