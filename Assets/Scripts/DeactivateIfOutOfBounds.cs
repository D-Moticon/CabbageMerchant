using UnityEngine;

public class DeactivateIfOutOfBounds : MonoBehaviour
{
    public bool deactivateIfBelowYBounds = true;

    public static float yBounds = -8f;

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < yBounds)
        {
            gameObject.SetActive(false);
        }
    }
}
