using System;
using UnityEngine;

public class MapCharacter : MonoBehaviour
{
    public SpriteRenderer sr;
    public Animator animator;
    public GameObject petParent;
    public SpriteRenderer petSprite;
    
    public void StartWalkingAnimation()
    {
        animator.SetBool("walking",true);
    }

    public void StopWalkingAnimation()
    {
        animator.SetBool("walking",false);
    }

    private void OnEnable()
    {
        if (Singleton.Instance.petManager.currentPet != null)
        {
            petParent.SetActive(true);
            petSprite.sprite = Singleton.Instance.petManager.currentPet.upSprite;
        }

        else
        {
            petParent.SetActive(false);
        }
    }
}
