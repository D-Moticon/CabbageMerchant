using System;
using UnityEngine;

public class FluidsOnOff : MonoBehaviour
{
    public GameObject fluidsParent;
    private void OnEnable()
    {
        if (Singleton.Instance.effectsManager.fluidsOn)
        {
            fluidsParent.SetActive(true);
        }

        else
        {
            print("off");
            fluidsParent.SetActive(false);
        }
    }
}
