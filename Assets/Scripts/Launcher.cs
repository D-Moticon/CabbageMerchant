using UnityEngine;
using UnityEngine.Serialization;

public class Launcher : MonoBehaviour
{
    PlayerInputManager playerInputManager;
    private ObjectPoolManager objectPoolManager;
    public PooledObjectData ballPooledObject;
    private Vector2 crosshairPos;
    public Crosshair crosshair;
    public float launchSpeed;
    [HideInInspector] public Vector2 currentLaunchVelocity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputManager = Singleton.Instance.playerInputManager;
        objectPoolManager = GameSingleton.Instance.objectPoolManager;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputManager == null)
        {
            playerInputManager = Singleton.Instance.playerInputManager;
        }

        if (objectPoolManager == null)
        {
            objectPoolManager = objectPoolManager = GameSingleton.Instance.objectPoolManager;
        }

        crosshairPos = crosshair.transform.position;
        
        Vector2 dir = (crosshairPos - (Vector2)this.transform.position).normalized;
        currentLaunchVelocity = dir * launchSpeed;
        
        
    }

    public void LaunchBall()
    {
        Ball ball = objectPoolManager.Spawn(ballPooledObject, this.transform.position, Quaternion.identity).GetComponent<Ball>();
        ball.SetVelocity(currentLaunchVelocity);
    }
}
