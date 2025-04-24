using UnityEngine;

public class SpriteMatFix : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            // somewhere in your setup loop—e.g. right after Instantiate(iconPrefab)—
            var sr = GetComponent<SpriteRenderer>();
            sr.material = new Material(sr.material);

    }

}
