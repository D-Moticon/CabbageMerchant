using System;
using UnityEngine;

public class OW_RunStartTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Singleton.Instance.menuManager.ShowPanel("RunStart");
    }
}
