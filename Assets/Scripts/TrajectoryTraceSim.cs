using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectoryTraceSim : MonoBehaviour
{
    private PhysicsScene2D _physicsSim;
    [SerializeField] private GameObject _simulatedObject;
    [SerializeField] private LineRenderer line;
    private Scene _simScene;

    [SerializeField] private int _steps = 20;
    private Vector2[] points;

    public Launcher launcher;
    public Transform targetTransform;
    private Vector2 previousAimPos;

    private List<GameObject> simCollidables = new List<GameObject>();


    private void OnEnable()
    {
        GameStateMachine.EnteringAimStateAction += EnteringAimStateListener;
        GameStateMachine.ExitingAimStateAction += ExitingAimStateListener;
    }

    private void OnDisable()
    {
        GameStateMachine.EnteringAimStateAction -= EnteringAimStateListener;
        GameStateMachine.ExitingAimStateAction -= ExitingAimStateListener;
    }

    private void Start()
    {
        CreateSceneParameters _param = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        _simScene = SceneManager.CreateScene("Simulation", _param);
        _physicsSim = _simScene.GetPhysicsScene2D();

        //CreateSimObjects();
        

        line.positionCount = _steps;
        points = new Vector2[_steps];

        EnableLine(true);
    }

    private void Update()
    {
        if ((previousAimPos - (Vector2)targetTransform.position).magnitude > 0.00001f)
        {
            SimulateLaunch(launcher.transform, launcher.currentLaunchVelocity);
        }
    }

    private void CreateSimObjects()
    {
        foreach (var simObj in simCollidables)
        {
            if (simObj != null)
                Destroy(simObj);
        }
        simCollidables.Clear();

        _simulatedObject.transform.SetParent(null);
        SceneManager.MoveGameObjectToScene(_simulatedObject, _simScene);

        GameObject[] collidables = GameObject.FindGameObjectsWithTag("Collidable");
        foreach (GameObject GO in collidables)
        {
            var newGO = Instantiate(GO, GO.transform.position, GO.transform.rotation);
            foreach (var spriteRenderer in newGO.GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.enabled = false;
            }

            foreach (Cabbage c in newGO.GetComponentsInChildren<Cabbage>())
            {
                c.enabled = false;
            }
            SceneManager.MoveGameObjectToScene(newGO, _simScene);
            simCollidables.Add(newGO);
        }
    }

    public void EnableLine(bool enabled)
    {
        line.gameObject.SetActive(enabled);
    }

    Vector2 _lastVel = Vector2.zero;
    public void SimulateLaunch(Transform player, Vector2 vel)
    {
        _simulatedObject.transform.position = player.position;
        _simulatedObject.transform.rotation = player.rotation;

        var simRb = _simulatedObject.GetComponent<Rigidbody2D>();
        simRb.linearVelocity = Vector2.zero;
        simRb.angularVelocity = 0f;

        if (_lastVel != vel)
        {
            simRb.linearVelocity = vel;
            for (var i = 0; i < _steps; i++)
            {
                _physicsSim.Simulate(Time.fixedDeltaTime);
                points[i] = _simulatedObject.transform.position;
                line.SetPosition(i, points[i]);
            }
        }

        _lastVel = vel;
        previousAimPos = targetTransform.position;
    }

    void EnteringAimStateListener()
    {
        CreateSimObjects();
        EnableLine(true);
    }

    void ExitingAimStateListener()
    {
        EnableLine(false);
    }
}