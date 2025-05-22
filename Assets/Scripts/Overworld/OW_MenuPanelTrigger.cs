using System;
using UnityEngine;

public class OW_MenuPanelTrigger : MonoBehaviour
{
    public string menuPanelName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OverworldCharacter owc = other.GetComponent<OverworldCharacter>();
        if (owc == null)
        {
            return;
        }

        if (!owc.isPlayer)
        {
            return;
        }
        
        owc.ForceStop();
        Singleton.Instance.menuManager.ShowPanel(menuPanelName);
    }
}
