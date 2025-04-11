using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputManager = Singleton.Instance.playerInputManager;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputManager != null)
        {
            //transform.position += (Vector3)playerInputManager.crosshairMove * Time.deltaTime;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(playerInputManager.mousePos);
            mousePos = new Vector3(mousePos.x, mousePos.y, 0f);
            transform.position = mousePos;
        }
    }
}
