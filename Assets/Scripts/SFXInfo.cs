using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[System.Serializable]
public class SFXInfo
{
    public EventReference sfx;
    public float vol = 0.5f;

    public void Play(Vector3 pos = default)
    {
        if (sfx.IsNull)
        {
            return;
        }

        if (pos.magnitude < 0.01f)
        {
            Singleton.Instance.audioManager.PlayOneShotCameraPosition(sfx, vol);
        }
        else
        {
            Singleton.Instance.audioManager.PlayOneShot(sfx, vol, pos);
        }
    }

    public void Play(Vector3 pos, float volume)
    {
        if (sfx.IsNull)
        {
            return;
        }

        Singleton.Instance.audioManager.PlayOneShot(sfx, volume, pos);
    }
    
    public FMOD.Studio.EventInstance Play(float duration, Vector3 pos = default)
    {
        if (sfx.IsNull)
        {
            return default(FMOD.Studio.EventInstance);
        }

        return Singleton.Instance.audioManager.PlayOneShot(sfx, vol, pos, duration);
    }

}

