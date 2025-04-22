using UnityEngine;
using UnityEngine.Serialization;
using MoreMountains.Feedbacks;
using Unity.Mathematics;

public class Launcher : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    public ObjectPoolManager objectPoolManager;
    public PooledObjectData ballPooledObject;
    private Vector2 crosshairPos;
    public float launchSpeed;
    [HideInInspector] public Vector2 currentLaunchVelocity;
    public MMF_Player launchFeel;
    public SFXInfo launchSFX;
    public ParticleSystem launchVFX;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputManager = Singleton.Instance.playerInputManager;
        if (objectPoolManager == null)
        {
            objectPoolManager = GameSingleton.Instance.objectPoolManager;
        }
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
        Ball ball = objectPoolManager.Spawn(ballPooledObject, this.transform.position, quaternion.identity).GetComponent<Ball>();
        ball.SetVelocity(currentLaunchVelocity);
        if (launchFeel != null)
        {
            launchFeel.PlayFeedbacks();
        }

        if (launchVFX != null)
        {
            launchVFX.Play();
        }

        launchSFX.Play();
        return ball;
    }
}
