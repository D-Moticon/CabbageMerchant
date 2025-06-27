using System.Collections;
using UnityEngine;

public class InstantiateObjectTask : DialogueTask
{
    public GameObject prefab;
    public Vector2 position;
    public float rotationAngle = 0;
    public Vector2 scale = new Vector2(1f,1f);
    
    public override IEnumerator RunTask(DialogueContext dc)
    {
        Quaternion rot = Helpers.AngleDegToRotation(rotationAngle);
        GameObject go = GameObject.Instantiate(prefab, position, rot);
        go.transform.localScale = scale;
        yield break;
    }
}
