using System;
using UnityEngine;

public class GoToSceneButton : MonoBehaviour
{
    public string sceneName;
    public bool exclusive = true;
    
    private void OnMouseDown()
    {
        if (exclusive)
        {
            Singleton.Instance.runManager.GoToSceneExclusive(sceneName);
        }

        else
        {
            Singleton.Instance.runManager.GoToScene(sceneName);
        }
        
        Singleton.Instance.menuManager.HideAll();
    }
}
