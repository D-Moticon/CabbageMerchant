using System;
using UnityEngine;

public class OW_MenuPanelTrigger : MonoBehaviour
{
    public string menuPanelName;
    private float enableTime;

    private void OnEnable()
    {
        enableTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - enableTime < 1f)
        {
            //prevent collisions from scene sliding
            return;
        }
        
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
