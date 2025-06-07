using UnityEngine;

public class ParticleSystemsPlayer : MonoBehaviour
{
    public ParticleSystem[] pSystems;
        
    public void PlayParticleSystem(int index)
    {
        pSystems[index].Play();
    }
}