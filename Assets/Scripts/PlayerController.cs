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
    public float bombSpawnDistance = 1f;
    
    [CanBeNull] public PlayerPlaceableLookAtData placeableLookAtData;
    
    public GameObject bombPrefab;
    public GameObject bombPreviewPrefab;

    private CharacterController characterController;
    private PlayerInput playerInput;

    [SerializeField] private Transform playerView;
    
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction placeBombAction;

    private Animator animator;
    private const float MaxLookAngle = 80f;
    
    private BoxCollider _placementCheckBox;
    private Camera _camera;

    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _placementCheckBox = GetComponent<BoxCollider>();
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        placeBombAction = playerInput.actions["PlaceBomb"];
        
        playerView = GetComponentInChildren<Camera>()?.transform;
        
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found for player");
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        UpdatePlacementLookAtData();
        HandleBombPlacement();
        
    }

    private void HandleBombPlacement()
    {
        if (placeBombAction.WasPressedThisFrame())
        {
            Debug.Log("stlacene");
            var bombPreview = Instantiate(bombPreviewPrefab, transform.position + (transform.forward * 1f), Quaternion.identity);
            bombPreview.GetComponent<SpawnPlaceableValidator>().Init(this.gameObject);
        }
    }

    void HandleMovement()
    {
        var moveInput = moveAction.ReadValue<Vector2>();
        var moveVector = transform.forward * moveInput.y + transform.right * moveInput.x;
        characterController.Move(moveVector * (Time.deltaTime * walkSpeed));
        
        float speed = moveInput.magnitude;
        if (animator)
        {
            animator.SetFloat("Speed", speed);
        }
    }

    void HandleLook()
    {
        var lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * sensitivity);
        pitch -= lookInput.y * Time.deltaTime * sensitivity;
        pitch = Mathf.Clamp(pitch, -MaxLookAngle, MaxLookAngle);
        playerView.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    
    private void UpdatePlacementLookAtData()
    {
        var hits = Physics.RaycastAll(_camera.transform.position, _camera.transform.forward, 100f);
        hits = hits.OrderBy(h => h.distance).ToArray();
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.isStatic)
            {
                placeableLookAtData = new PlayerPlaceableLookAtData(transform.position, hit.point, hit.collider.gameObject);
                return;
            }
        }
        placeableLookAtData = null;
        
        
    }


}

public class PlayerPlaceableLookAtData
{
    public PlayerPlaceableLookAtData(Vector3 playerPosition, Vector3 staticObjectIntersectPosition, GameObject intersectedObject)
    {
        this.playerPosition = playerPosition;
        this.staticObjectIntersectPosition = staticObjectIntersectPosition;
        IntersectedObject = intersectedObject;
    }

    public Vector3 playerPosition;
    public Vector3 staticObjectIntersectPosition;
    public GameObject IntersectedObject;
}