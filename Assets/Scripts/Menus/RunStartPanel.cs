using UnityEngine;

public class RunStartPanel : MenuPanel
{
    public void StartRun(Difficulty difficulty)
    {
        Singleton.Instance.runManager.StartNewRun();
        Singleton.Instance.menuManager.HideAll();
    }
}
