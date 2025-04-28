using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple component to loop through a list of sprites on a SpriteRenderer.
/// Attach to a GameObject with a SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteLoopAnimator : MonoBehaviour
{
    [Header("Animation Frames")]
    [Tooltip("List of sprites to cycle through in order.")]
    public List<Sprite> frames = new List<Sprite>();

    [Header("Timing")]
    [Tooltip("Frames per second to play the animation.")]
    public float framesPerSecond = 12f;

    [Header("Looping")]
    [Tooltip("Whether to start playing automatically on Awake.")]
    public bool playOnAwake = true;

    [Tooltip("Whether the animation should loop when it reaches the end.")]
    public bool loop = true;

    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex;
    private float timer;
    private float frameDuration;
    private bool isPlaying;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        frameDuration = framesPerSecond > 0 ? 1f / framesPerSecond : Mathf.Infinity;
    }

    void Start()
    {
        if (playOnAwake)
            Play();
    }

    void Update()
    {
        if (!isPlaying || frames == null || frames.Count == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            AdvanceFrame();
        }
    }

    /// <summary>
    /// Starts or restarts the animation from the first frame.
    /// </summary>
    public void Play()
    {
        if (frames == null || frames.Count == 0)
            return;

        isPlaying = true;
        currentFrameIndex = 0;
        timer = 0f;
        spriteRenderer.sprite = frames[0];
    }

    /// <summary>
    /// Stops the animation, keeping the current frame displayed.
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Advances to the next frame, looping or stopping at the end.
    /// </summary>
    private void AdvanceFrame()
    {
        currentFrameIndex++;
        if (currentFrameIndex >= frames.Count)
        {
            if (loop)
                currentFrameIndex = 0;
            else
            {
                currentFrameIndex = frames.Count - 1;
                isPlaying = false;
            }
        }

        spriteRenderer.sprite = frames[currentFrameIndex];
    }
}