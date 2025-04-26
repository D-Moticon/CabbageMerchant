using System;
using UnityEngine;

public class QuitGameButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        Singleton.Instance.saveManager.SaveToDisk();
        Application.Quit();
    }
}
