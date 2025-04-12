using UnityEngine;

public class MapIcon : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D bc2d;

    // If you want an easy way to store the scene reference on the icon:
    [HideInInspector] public string sceneName = "";

    private void OnMouseDown()
    {
        MapSingleton.Instance.mapManager.OnMapIconClicked(this);
    }
}