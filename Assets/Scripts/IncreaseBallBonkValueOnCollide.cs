using System;
using UnityEngine;

public class IncreaseBallBonkValueOnCollide : MonoBehaviour
{
    public float bonkValueAdd = 0.1f;
    public FloaterReference floaterReference;
    public Color floaterColor = Color.white;
    public float floaterScale = 0.75f;
    private void OnCollisionEnter2D(Collision2D other)
    {
        Ball b = other.gameObject.GetComponent<Ball>();
        if (b == null)
        {
            return;
        }
        
        b.AddBonkValue(bonkValueAdd);
        //floaterReference.Spawn($"+{bonkValueAdd:F1}", b.transform.position, floaterColor, floaterScale);
    }
}
