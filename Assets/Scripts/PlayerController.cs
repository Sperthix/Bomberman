using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public int walkSpeed = 15;
    public int sensitivity = 10;
    public int bombSpawnDistance = 1;
    
    public GameObject bombPrefab;

    private CharacterController characterController;
    private PlayerInput playerInput;
    
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction placeBombAction;
    
    

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        placeBombAction = playerInput.actions["PlaceBomb"];
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleBombPlacement();
    }

    private void HandleBombPlacement()
    {
        if (placeBombAction.WasPerformedThisFrame())
        {
            PlaceBomb();
        }
    }

    void HandleMovement()
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVector = transform.forward * moveInput.y + transform.right * moveInput.x;
        characterController.Move(moveVector * (Time.deltaTime * walkSpeed));
    }

    void HandleLook()
    {
        var lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * sensitivity);
    }

    void PlaceBomb()
    {
        if (bombPrefab is null)
        {
            Debug.LogWarning("Nemam bombu");
            return;
        }
        
        Vector3 center = characterController.bounds.center;
        Vector3 forward = transform.forward;
        Vector3 PlacePos = center + forward * bombSpawnDistance;
        PlacePos.y = 0.5f;
        
        Instantiate(bombPrefab, PlacePos, Quaternion.identity);
    }
}