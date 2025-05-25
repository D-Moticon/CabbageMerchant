using System;
using UnityEngine;
using TMPro;

public class VersionText : MonoBehaviour
{
    public TMP_Text text;
    private void OnEnable()
    {
        text.text = Singleton.Instance.buildManager.version;
    }
}
