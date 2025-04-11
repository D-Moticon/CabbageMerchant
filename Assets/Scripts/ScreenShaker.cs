using MoreMountains.Feedbacks;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    public MMF_Player shakePlayer;
    public void ShakeScreen()
    {
        shakePlayer.PlayFeedbacks();
    }
}
