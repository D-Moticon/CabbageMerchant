using System;
using UnityEngine;
using System.Collections.Generic;

public class DemoOnlyObjects : MonoBehaviour
{
    public List<GameObject> demoObjects;
    public List<GameObject> demoOffObjects;
    
    private void OnEnable()
    {
        if (Singleton.Instance.buildManager.buildMode != BuildManager.BuildMode.demo)
        {
            foreach (GameObject go in demoObjects)
            {
                go.SetActive(false);
            }
            
            foreach (GameObject go in demoOffObjects)
            {
                go.SetActive(true);
            }
        }

        else
        {
            foreach (GameObject go in demoObjects)
            {
                go.SetActive(true);
            }
            
            foreach (GameObject go in demoOffObjects)
            {
                go.SetActive(false);
            }
        }
    }
}
