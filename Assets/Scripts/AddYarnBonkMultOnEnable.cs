using System;
using UnityEngine;

public class AddYarnBonkMultOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        Cabbage c = GetComponent<Cabbage>();
        if (c == null)
        {
            return;
        }
        
        c.AddBonkMultiplier(Singleton.Instance.playerStats.catYarnBallPointAdd);
    }
}
