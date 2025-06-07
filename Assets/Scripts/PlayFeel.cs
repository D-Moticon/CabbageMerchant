using UnityEngine;
using MoreMountains.Feedbacks;

public class PlayFeels : MonoBehaviour
{
    public MMF_Player[] feelPlayers;

    public void PlayFeel(int index)
    {
        feelPlayers[index].PlayFeedbacks();
    }
}
