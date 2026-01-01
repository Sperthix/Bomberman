using System;
using System.Linq;
using JetBrains.Annotations;
using UI.InGamePlayerHud;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
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
    private InputAction abilitySelectedAction;

    private Animator animator;
    
    public NetworkVariable<Vector3> spawnPosition = new NetworkVariable<Vector3>(
        Vector3.zero, 
        NetworkVariableReadPermission.Owner
    );


    public Camera PlayerCamera { get; private set; }

    void Start()
    {
        PlayerCamera = GetComponentInChildren<Camera>();
        
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        if (!IsOwner)
        {
            PlayerCamera.gameObject.SetActive(false);
            playerInput.enabled = false;
            return;
        }
        
        spawnPosition.OnValueChanged += (_, newValue) =>
        {
            if (!IsOwner) return;
            characterController.enabled = false;
            transform.position = newValue;
            characterController.enabled = true;
        };
        
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        placeBombAction = playerInput.actions["PlaceBomb"];
        abilitySelectedAction = playerInput.actions["AbilitySelected"];
        animator = GetComponentInChildren<Animator>();
        
        InGamePlayerHudManager.Instance.BindPlayer(gameObject);
        
        NotifyBombSelectionChanged();
        
        GameStateServerAPI.Instance.ClientPlayerSpawnedServerRpc();
    }

    void Update()
    {
        if (!IsOwner) return;
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
        if (!abilitySelectedAction.WasPressedThisFrame()) return; 
        var abilitySelected = (int)abilitySelectedAction.ReadValue<float>();
        
        SetBombIndex(abilitySelected);
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