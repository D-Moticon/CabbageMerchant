using System;
using UnityEngine;

public class CharacterSkinSprite : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public enum SpriteDir
    {
        up,
        down,
        left,
        right
    }

    public SpriteDir spriteDir;
    
    private void OnEnable()
    {
        switch (spriteDir)
        {
            case SpriteDir.up:
                spriteRenderer.sprite = Singleton.Instance.skinManager.currentSkin.upSprite;
                break;
            case SpriteDir.down:
                spriteRenderer.sprite = Singleton.Instance.skinManager.currentSkin.downSprite;
                break;
            case SpriteDir.left:
                spriteRenderer.sprite = Singleton.Instance.skinManager.currentSkin.leftSprite;
                break;
            case SpriteDir.right:
                spriteRenderer.sprite = Singleton.Instance.skinManager.currentSkin.rightSprite;
                break;
        }
        
    }
}
