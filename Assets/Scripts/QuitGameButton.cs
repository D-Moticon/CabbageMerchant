using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuitGameButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        Singleton.Instance.saveManager.SaveToDisk();
        
        Application.Quit();
    }
}
