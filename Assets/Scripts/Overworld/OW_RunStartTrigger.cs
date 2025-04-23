using System;
using UnityEngine;

public class OW_RunStartTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        OverworldCharacter owc = other.GetComponent<OverworldCharacter>();
        if (owc == null)
        {
            return;
        }
        Singleton.Instance.menuManager.ShowPanel("RunStart");
    }
}
