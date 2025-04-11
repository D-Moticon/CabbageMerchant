using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        if (sound.IsNull)
        {
            return;
        }

        if (Vector2.Distance(worldPos, (Vector2)Camera.main.transform.position) > 30f)
        {
            return;
        }

        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void PlayOneShot(EventReference sound, float volume, Vector3 worldPos)
    {
        if (sound.IsNull)
        {
            return;
        }

        //volume = volume - (worldPos - Camera.main.transform.position).magnitude * 0.01f;
        if (Vector3.Distance(worldPos, Camera.main.transform.position) > 50f)
        {
            return;
        }

        volume = Mathf.Clamp(volume, 0.0f, 1.0f);

        RuntimeManager.PlayOneShot(sound, volume, worldPos);
    }

    public void PlayOneShotCameraPosition(EventReference sound, float volume)
    {
        if (sound.IsNull)
        {
            return;
        }

        PlayOneShot(sound, volume, Camera.main.transform.position);
    }
}