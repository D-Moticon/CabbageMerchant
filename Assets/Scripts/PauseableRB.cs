using System;
using UnityEngine;

public class PauseableRB : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 prePauseVelocity;

    private void Awake()
    {
        if(rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        PauseManager.GamePausedEvent += GamePausedListener;
        PauseManager.GameUnPausedEvent += GameUnPausedListener;
    }

    private void OnDisable()
    {
        PauseManager.GamePausedEvent -= GamePausedListener;
        PauseManager.GameUnPausedEvent -= GameUnPausedListener;
    }

    private void GamePausedListener()
    {
        prePauseVelocity = rb.linearVelocity;
        rb.simulated = false;
    }
    
    private void GameUnPausedListener()
    {
        rb.linearVelocity = prePauseVelocity;
        rb.simulated = true;
    }
}
