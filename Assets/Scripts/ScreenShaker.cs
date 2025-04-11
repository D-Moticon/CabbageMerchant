using MoreMountains.Feedbacks;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    public MMF_Player shakePlayer;
    public void ShakeScreen(float intensity = 1f)
    {
        shakePlayer.PlayFeedbacks(this.transform.position, intensity);
    }
}
