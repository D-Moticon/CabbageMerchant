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

        // Unload the simulation scene if it is still valid
        if (_simScene.IsValid())
        {
            SceneManager.UnloadSceneAsync(_simScene);
        }
    }

    private void Start()
    {
        CreateSceneParameters _param = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        _simScene = SceneManager.CreateScene("Simulation", _param);
        _physicsSim = _simScene.GetPhysicsScene2D();

        // We can create the sim objects here or in EnteringAimStateListener if you prefer
        // CreateSimObjects();

        line.positionCount = _steps;
        points = new Vector2[_steps];

        EnableLine(true);
    }

    private void Update()
    {
        // If the aim position has changed, recompute the trajectory
        if ((previousAimPos - (Vector2)targetTransform.position).magnitude > 0.00001f)
        {
            SimulateLaunch();
        }
    }

    private void CreateSimObjects()
    {
        // Clean up old objects if re-creating
        foreach (var simObj in simCollidables)
        {
            if (simObj != null)
            {
                Destroy(simObj);
            }
        }
        simCollidables.Clear();

        // Move the simulatedObject to the simulation scene
        _simulatedObject.transform.SetParent(null);
        SceneManager.MoveGameObjectToScene(_simulatedObject, _simScene);

        // Duplicate any collidables for collision simulation
        GameObject[] collidables = GameObject.FindGameObjectsWithTag("Collidable");
        foreach (GameObject GO in collidables)
        {
            var newGO = Instantiate(GO, GO.transform.position, GO.transform.rotation);

            // Disable visuals so they won't appear in the real scene
            foreach (var renderer in newGO.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
            foreach (var tmp in newGO.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                tmp.enabled = false;
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
    public void SimulateLaunch()
    {
        Transform player = launcher.transform;
        Vector2 vel = launcher.currentLaunchVelocity;

        // Reset the simulated object
        _simulatedObject.transform.position = player.position;
        _simulatedObject.transform.rotation = player.rotation;

        var simRb = _simulatedObject.GetComponent<Rigidbody2D>();
        simRb.linearVelocity = Vector2.zero;
        simRb.angularVelocity = 0f;

        // Only update if velocity changed
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
        SimulateLaunch();
    }

    void ExitingAimStateListener()
    {
        EnableLine(false);
    }
}
