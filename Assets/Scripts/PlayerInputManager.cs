using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInputManager : MonoBehaviour
{
    public float crosshairSens = 5f;
    
    [SerializeField]private InputActionReference crosshairMoveInput;
    [SerializeField] private InputActionReference mousePosInput;
    [SerializeField] private InputActionReference fireInput;
    [SerializeField] private InputActionReference weaponFireInput;
    [SerializeField] private InputActionReference dialogueSkipInput;
    [SerializeField] private InputActionReference pauseInput;
    [SerializeField] private InputActionReference mapScrollInput;

    public Vector2 crosshairMove;
    public Crosshair crosshair;
    public Vector2 mousePos;
    public Vector2 mousePosWorldSpace;
    public bool fireDown;
    public bool fireHeld;
    public bool fireUp;
    public bool weaponFireDown;
    public bool weaponFireHeld;
    public bool weaponFireUp;
    public bool dialogueSkipDown;
    public bool pauseDown;
    public float mapScroll;

    public static System.Action fireDownAction;
    public static System.Action fireUpAction;
    
    public static System.Action weaponFireDownAction;
    public static System.Action weaponFireUpAction;

    public static System.Action pauseDownAction;
    
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

        weaponFireDown = weaponFireInput.action.WasPressedThisFrame();
        weaponFireHeld = weaponFireInput.action.IsPressed();
        weaponFireUp = weaponFireInput.action.WasReleasedThisFrame();

        pauseDown = pauseInput.action.WasPressedThisFrame();

        mapScroll = mapScrollInput.action.ReadValue<float>();

        if (fireDown)
        {
            fireDownAction?.Invoke();
        }

        if (fireUp)
        {
            fireUpAction?.Invoke();
        }
        
        if (weaponFireDown)
        {
            weaponFireDownAction?.Invoke();
        }

        if (weaponFireUp)
        {
            weaponFireUpAction?.Invoke();
        }

        if (pauseDown)
        {
            pauseDownAction?.Invoke();
        }

        dialogueSkipDown = dialogueSkipInput.action.WasPressedThisFrame();
    }
}
