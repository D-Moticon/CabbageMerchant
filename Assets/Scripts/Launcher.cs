using System;
using UnityEngine;
using UnityEngine.Serialization;
using MoreMountains.Feedbacks;
using Unity.Mathematics;

public class Launcher : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    public bool isGameLauncher = false;
    public ObjectPoolManager objectPoolManager;
    public PooledObjectData ballPooledObject;
    public PooledObjectData rainbowBallPooledObject;
    public PhysicsMaterial2D defaultPhysicsMat;
    private Vector2 crosshairPos;
    public float baseLaunchSpeed = 10f;
    public float launchSpeed = 10f;
    [HideInInspector] public Vector2 currentLaunchVelocity;
    public MMF_Player launchFeel;
    public SFXInfo launchSFX;
    public ParticleSystem launchVFX;

    private void OnEnable()
    {
        GameStateMachine.EnteringAimStateAction += OnAimStateEnter;
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringAimStateAction -= OnAimStateEnter;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputManager = Singleton.Instance.playerInputManager;
        if (objectPoolManager == null)
        {
            objectPoolManager = GameSingleton.Instance.objectPoolManager;
        }

        launchSpeed = baseLaunchSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputManager == null)
        {
            playerInputManager = Singleton.Instance.playerInputManager;
        }

        crosshairPos = Singleton.Instance.playerInputManager.crosshair.transform.position;
        
        Vector2 dir = (crosshairPos - (Vector2)this.transform.position).normalized;
        currentLaunchVelocity = dir * launchSpeed;
        
        
    }

    public Ball LaunchBall()
    {
        PooledObjectData ballToSpawn = ballPooledObject;
        
        if (Singleton.Instance.launchModifierManager.forceNextBallRainbow && isGameLauncher)
        {
            ballToSpawn = rainbowBallPooledObject;
        }
        
        Ball ball = objectPoolManager.Spawn(ballToSpawn, this.transform.position, quaternion.identity).GetComponent<Ball>();
        ball.SetScale(1f);
        ball.SetVelocity(currentLaunchVelocity);
        if (defaultPhysicsMat != null)
        {
            ball.rb.sharedMaterial = defaultPhysicsMat;
            ball.col.sharedMaterial = defaultPhysicsMat;
        }
        
        if (launchFeel != null)
        {
            launchFeel.PlayFeedbacks();
        }

        if (launchVFX != null)
        {
            launchVFX.Play();
        }

        if (Singleton.Instance.launchModifierManager.forceNextBallRainbow)
        {
            ball.bonkValue = Singleton.Instance.launchModifierManager.forceNextBallBonkValue;
        }

        launchSFX.Play();
        
        return ball;
    }

    public void SetLaunchSpeed(float newSpeed)
    {
        launchSpeed = newSpeed;
    }

    public void SetLaunchSpeedToDefault()
    {
        launchSpeed = baseLaunchSpeed;
    }

    public void AddLaunchSpeed(float speedAdd)
    {
        launchSpeed += speedAdd;
    }

    void OnAimStateEnter()
    {
        
    }
}
