using System;
using UnityEngine;

public class GameCharacterSprite : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        spriteRenderer.sprite = Singleton.Instance.skinManager.currentSkin.dialogueSprite;
    }
}
