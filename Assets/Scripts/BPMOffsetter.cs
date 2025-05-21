using System;
using UnityEngine;

public class BPMOffsetter : MonoBehaviour
{
    public float offsetPerObject = .25f;
    
    private void Awake()
    {
        IBPM[] bpms = GetComponentsInChildren<IBPM>();
        for (int i = 0; i < bpms.Length; i++)
        {
            bpms[i].OffsetBeat(offsetPerObject*i);
        }
    }
}
