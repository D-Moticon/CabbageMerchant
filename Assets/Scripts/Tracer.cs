using UnityEngine;
using System.Collections;

public class Tracer : MonoBehaviour
{
    public AnimationCurve arcCurve = AnimationCurve.Linear(0f,0f,1f,1f);
    public SpriteRenderer sr;
    public TrailRenderer tr;
    
    public void StartTracer(Vector2 startPoint, Vector2 endPoint, float duration = 0.5f, float arcDistance=1f, Sprite sprite = null, Color color = default)
    {
        StopAllCoroutines();
        sr.sprite = sprite;
        tr.startColor = color;
        tr.endColor = color;
        StartCoroutine(TracerCoroutine(startPoint, endPoint, duration, arcDistance));
    }

    IEnumerator TracerCoroutine(Vector2 startPoint, Vector2 endPoint, float duration, float arcDistance=1f)
    {
        float elapsedTime = Time.deltaTime;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 pos = Vector2.Lerp(startPoint, endPoint, t)
                +arcDistance*arcCurve.Evaluate(t)*Vector2.up;
            transform.position = pos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public static void SpawnTracer(PooledObjectData tracer, Vector2 startPoint, Vector2 endPoint, float duration = 0.5f, float arcDistance = 1f, Sprite sprite = null, Color color = default)
    {
        Tracer t = tracer.Spawn(startPoint).GetComponent<Tracer>();
        if (t == null)
        {
            print("Tried to spawn tracer with no tracer component");
        }
        
        t.StartTracer(startPoint, endPoint, duration, arcDistance, sprite, color);
    }
}
