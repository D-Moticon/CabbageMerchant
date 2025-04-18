using UnityEngine;

public class SFX_Player : MonoBehaviour
{
    public SFXInfo sfx;

    public void PlaySFX()
    {
        sfx.Play();
    }
}
