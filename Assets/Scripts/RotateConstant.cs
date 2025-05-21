using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RotateConstant : MonoBehaviour
{
    public Vector2 speedRange = new Vector2(0f, 360f);
    public Vector2 randomStartRange = new Vector2(0f, 360f);
    private float speed;

    private void Start()
    {
        float ang = Random.Range(randomStartRange.x, randomStartRange.y);
        transform.rotation = Helpers.AngleDegToRotation(ang);
        speed = Random.Range(speedRange.x, speedRange.y);
    }

    void Update()
    {
        // get current Z rotation in degrees
        float currentAngle = transform.eulerAngles.z;
        // add delta
        float newAngle     = currentAngle + speed * Time.deltaTime;
        // re-apply
        transform.rotation = Helpers.AngleDegToRotation(newAngle);
    }

}
