using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using STOP_MODE = FMOD.Studio.STOP_MODE;

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
    
    /// <summary>
    /// Plays the sound at worldPos, but only for duration seconds (then stops immediately).
    /// </summary>
    public void PlayOneShot(EventReference sound, Vector3 worldPos, float duration)
    {
        if (sound.IsNull)
            return;

        if (Vector2.Distance(worldPos, (Vector2)Camera.main.transform.position) > 30f)
            return;

        // create a one‑shot instance instead of the helper
        var instance = RuntimeManager.CreateInstance(sound);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        instance.start();

        // schedule its stop
        StartCoroutine(StopAfter(instance, duration, STOP_MODE.IMMEDIATE));
    }

    /// <summary>
    /// Same as above, but lets you specify a volume as well.
    /// </summary>
    public EventInstance PlayOneShot(EventReference sound, float volume, Vector3 worldPos, float duration)
    {
        if (sound.IsNull)
            return default(EventInstance);

        if (Vector3.Distance(worldPos, Camera.main.transform.position) > 50f)
            return default(EventInstance);

        volume = Mathf.Clamp01(volume);

        var instance = RuntimeManager.CreateInstance(sound);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        instance.setVolume(volume);
        instance.start();

        StartCoroutine(StopAfter(instance, duration, STOP_MODE.IMMEDIATE));

        return instance;
    }

    private IEnumerator StopAfter(EventInstance instance, float delay, STOP_MODE mode)
    {
        yield return new WaitForSeconds(delay);
        instance.stop(mode);
        instance.release();
    }
}