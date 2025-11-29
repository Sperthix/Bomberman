using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 15f;
    public float sensitivity = 10f;
    private float pitch = 0f;
    private const float MaxLookAngle = 80f;
    
    [CanBeNull] public PlayerPlaceableLookAtData PlaceableLookAtData;

    public GameObject[] bombPreviewPrefabs;
    public int SelectedBombPreviewIndex { get; private set; } = 0;

    public event Action<int, int> OnBombSelectionChanged; // (currentIndex, totalCount)

    private CharacterController characterController;
    private PlayerInput playerInput;
    
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction placeBombAction;

    private Animator animator;

    public Camera PlayerCamera { get; private set; }

    void Start()
    {
        PlayerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        placeBombAction = playerInput.actions["PlaceBomb"];
        animator = GetComponentInChildren<Animator>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        NotifyBombSelectionChanged();
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        UpdatePlacementLookAtData();
        HandleBombSelection();
        HandleBombPlacement();
    }

    private void HandleBombPlacement()
    {
        if (placeBombAction.WasPressedThisFrame())
        {
            var bombPreview = Instantiate(bombPreviewPrefabs[SelectedBombPreviewIndex], transform.position + (transform.forward * 1f), Quaternion.identity);
            bombPreview.GetComponent<SpawnPlaceableValidator>().Init(this.gameObject);
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
        if (bombPreviewPrefabs == null || bombPreviewPrefabs.Length == 0) return;
        if (index < 0 || index >= bombPreviewPrefabs.Length) return;
        if (index == SelectedBombPreviewIndex) return;

        SelectedBombPreviewIndex = index;
        NotifyBombSelectionChanged();
    }

    private void NotifyBombSelectionChanged()
    {
        OnBombSelectionChanged?.Invoke(SelectedBombPreviewIndex, bombPreviewPrefabs?.Length ?? 0);
    }

    private void HandleMovement()
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVector = transform.forward * moveInput.y + transform.right * moveInput.x;
        characterController.Move(moveVector * (Time.deltaTime * walkSpeed));

        float speed = moveInput.magnitude;
        animator.SetFloat("Speed", speed);
    }

    private void HandleLook()
    {
        var lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * sensitivity);
        pitch -= lookInput.y * Time.deltaTime * sensitivity;
        pitch = Mathf.Clamp(pitch, -MaxLookAngle, MaxLookAngle);
        PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    
    private void UpdatePlacementLookAtData()
    {
        var hits = Physics.RaycastAll(PlayerCamera.transform.position, PlayerCamera.transform.forward, 100f);
        hits = hits.OrderBy(h => h.distance).ToArray();
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.isStatic)
            {
                PlaceableLookAtData = new PlayerPlaceableLookAtData(transform.position, hit.point, hit.collider.gameObject, hit.normal);
                return;
            }
        }
        PlaceableLookAtData = null;
    }
}

public class PlayerPlaceableLookAtData
{
    public PlayerPlaceableLookAtData(Vector3 playerPosition, Vector3 staticObjectIntersectPosition,
        GameObject intersectedObject, Vector3 hitNormal)
    {
        this.playerPosition = playerPosition;
        this.staticObjectIntersectPosition = staticObjectIntersectPosition;
        IntersectedObject = intersectedObject;
        IntersectNormalVec = hitNormal;
    }

    public Vector3 playerPosition;
    public Vector3 staticObjectIntersectPosition;
    public GameObject IntersectedObject;
    public Vector3 IntersectNormalVec;
}