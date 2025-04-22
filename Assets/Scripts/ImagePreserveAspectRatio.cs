using UnityEngine;
using UnityEngine.UI;

public class ImagePreserveAspectRatio : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Image img = GetComponent<Image>();
        img.preserveAspect = true;
    }
}
