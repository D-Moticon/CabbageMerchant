using UnityEngine;

public class MapCharacter : MonoBehaviour
{
    public SpriteRenderer sr;
    public Animator animator;
    
    public void StartWalkingAnimation()
    {
        animator.SetBool("walking",true);
    }

    public void StopWalkingAnimation()
    {
        animator.SetBool("walking",false);
    }
}
