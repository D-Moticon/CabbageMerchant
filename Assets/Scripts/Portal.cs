using System;
using UnityEngine;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    public Portal otherPortal;
    private List<GameObject> objectsThatJustTPdHere = new List<GameObject>();
    public PooledObjectData objectExitVFX;
    public SFXInfo objectExitSFX;
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (objectsThatJustTPdHere.Contains(other.gameObject))
        {
            return;
        }
        other.transform.position = otherPortal.transform.position;
        otherPortal.objectsThatJustTPdHere.Add(other.gameObject);
        objectExitVFX.Spawn(other.transform.position);
        objectExitSFX.Play();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (objectsThatJustTPdHere.Contains(other.gameObject))
        {
            objectsThatJustTPdHere.Remove(other.gameObject);
        }
    }
}
