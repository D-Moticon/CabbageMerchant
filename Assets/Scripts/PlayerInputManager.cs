using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInputManager : MonoBehaviour
{
    public float crosshairSens = 5f;
    
    [SerializeField]private InputActionReference crosshairMoveInput;
    [SerializeField] private InputActionReference mousePosInput;
    [SerializeField] private InputActionReference fireInput;

    public Vector2 crosshairMove;
    public Vector2 mousePos;
    public Vector2 mousePosWorldSpace;
    public bool fireDown;
    public bool fireHeld;
    public bool fireUp;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        crosshairMoveInput.action.Enable();
        mousePosInput.action.Enable();
        fireInput.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        crosshairMove = crosshairMoveInput.action.ReadValue<Vector2>() * crosshairSens;
        mousePos = mousePosInput.action.ReadValue<Vector2>();
        mousePosWorldSpace = Camera.main.ScreenToWorldPoint(mousePos);
        
        fireDown = fireInput.action.WasPressedThisFrame();
        fireHeld = fireInput.action.IsPressed();
        fireUp = fireInput.action.WasReleasedThisFrame();
    }
}
