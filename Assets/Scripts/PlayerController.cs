using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public int walkSpeed = 15;
    public int sensitivity = 10;

    private CharacterController CharacterController;
    private InputAction moveAction;
    private InputAction lookAction;

    void Start()
    {
        CharacterController = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVector = transform.forward * moveInput.y + transform.right * moveInput.x;
        CharacterController.Move(moveVector * Time.deltaTime * walkSpeed);

        var lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * sensitivity);
    }
}