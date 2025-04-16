using UnityEngine;

public class NewRunButton : MonoBehaviour
{
    public void StartNewRun()
    {
        Singleton.Instance.runManager.StartNewRun();
    }
}
