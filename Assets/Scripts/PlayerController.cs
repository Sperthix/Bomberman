using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 15f;
    public float sensitivity = 10f;
    public float bombSpawnDistance = 1f;
    private float pitch = 0f;
    private const float MaxLookAngle = 80f;
    
    public GameObject[] bombPrefabs;
    
    public int SelectedBombIndex { get; private set; } = 0;
    public event Action<int, int> OnBombSelectionChanged; // (currentIndex, totalCount)

    private CharacterController characterController;
    private PlayerInput playerInput;

    [SerializeField] private Transform playerView;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction placeBombAction;

    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        placeBombAction = playerInput.actions["PlaceBomb"];
        playerView = GetComponentInChildren<Camera>()?.transform;
        animator = GetComponentInChildren<Animator>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        NotifyBombSelectionChanged();
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleBombSelection();
        HandleBombPlacement();
    }

    private void HandleBombPlacement()
    {
        if (placeBombAction.WasPerformedThisFrame())
        {
            PlaceBomb();
        }
    }

    private void HandleBombSelection()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard.digit1Key.wasPressedThisFrame) SetBombIndex(0);
        if (keyboard.digit2Key.wasPressedThisFrame) SetBombIndex(1);
        if (keyboard.digit3Key.wasPressedThisFrame) SetBombIndex(2);
    }

    private void SetBombIndex(int index)
    {
        if (bombPrefabs == null || bombPrefabs.Length == 0) return;
        if (index < 0 || index >= bombPrefabs.Length) return;
        if (index == SelectedBombIndex) return;

        SelectedBombIndex = index;
        NotifyBombSelectionChanged();
    }

    private void NotifyBombSelectionChanged()
    {
        OnBombSelectionChanged?.Invoke(SelectedBombIndex, bombPrefabs?.Length ?? 0);
    }

    void HandleMovement()
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVector = transform.forward * moveInput.y + transform.right * moveInput.x;
        characterController.Move(moveVector * (Time.deltaTime * walkSpeed));

        float speed = moveInput.magnitude;
        animator.SetFloat("Speed", speed);
    }

    void HandleLook()
    {
        var lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * sensitivity);
        pitch -= lookInput.y * Time.deltaTime * sensitivity;
        pitch = Mathf.Clamp(pitch, -MaxLookAngle, MaxLookAngle);
        playerView.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void PlaceBomb()
    {
        var prefab = bombPrefabs[Mathf.Clamp(SelectedBombIndex, 0, bombPrefabs.Length - 1)];
        Vector3 center = characterController.bounds.center;
        Vector3 forward = transform.forward;
        Vector3 placePos = center + forward * bombSpawnDistance;
        placePos.y = 0.5f;

        Instantiate(prefab, placePos, Quaternion.identity);
    }
}
