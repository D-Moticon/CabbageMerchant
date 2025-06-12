using UnityEngine;
using MoreMountains.Feedbacks;

public class Crosshair : MonoBehaviour
{
    private PlayerInputManager playerInputManager;

    public SpriteRenderer spriteRenderer;
    
    public Sprite defaultSprite;
    public Sprite grabSprite;
    public Sprite weaponFireSprite;

    public MMF_Player clickFeel;
    public PooledObjectData clickVFX;
    
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

            if (Singleton.Instance.itemManager.IsDraggingItem())
            {
                spriteRenderer.sprite = grabSprite;
            }
            
            else if (playerInputManager.weaponFireHeld)
            {
                spriteRenderer.sprite = weaponFireSprite;
            }

            else
            {
                spriteRenderer.sprite = defaultSprite;
            }

            if (playerInputManager.fireDown)
            {
                clickFeel.PlayFeedbacks();
                clickVFX.Spawn(this.transform.position);
            }
        }

        Cursor.visible = false;
    }
}
