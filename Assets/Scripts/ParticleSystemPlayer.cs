using UnityEngine;

public class ParticleSystemPlayer : MonoBehaviour
{
    public ParticleSystem pSystem;

    public void PlayParticleSystem()
    {
        pSystem.Play();
    }
}
