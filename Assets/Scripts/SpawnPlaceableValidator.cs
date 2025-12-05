using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPlaceableValidator : NetworkBehaviour
{
    private GameObject _playerRef;
    private PlayerController _playerControllerRef;
    public GameObject bombPrefab;

    private PlayerInput _playerInput;
    private InputAction _placeBombAction;

    private Material _defaultMaterial;
    public Material redTransparentMaterial;

    private Renderer _renderer;
    private int _maxPlacementDistance;

    private bool _isInitialized = false;

    void Update()
    {
        if (!_isInitialized) return;

        var playerPlaceableLookAtData = _playerControllerRef.PlaceableLookAtData;
        if (playerPlaceableLookAtData == null)
        {
            ShowInvalidPreviewNearPlayerCam();
            if (_placeBombAction.WasReleasedThisFrame()) Destroy(gameObject);
            return;
        }


        var placementPosition = CalcualtePlacementPosition(playerPlaceableLookAtData);
        transform.position = placementPosition;

        var isColliding = IsSpawnPointColliding();
        var isOutOfRange = IsOutOfRange(placementPosition, playerPlaceableLookAtData.playerPosition);
        var isSpawnPlaceValid = !isColliding && !isOutOfRange;

        if (!isSpawnPlaceValid)
        {
            _renderer.material = redTransparentMaterial;
        }
        else
        {
            _renderer.material = _defaultMaterial;
        }

        if (_placeBombAction.WasReleasedThisFrame())
        {
            if (isSpawnPlaceValid)
            {
                PlaceBomb();
            }

            Destroy(gameObject);
        }
    }

    private void ShowInvalidPreviewNearPlayerCam()
    {
        var transformPosition = _playerControllerRef.PlayerCamera.transform.position +
                                _playerControllerRef.PlayerCamera.transform.forward * 1f;
        transformPosition += _playerControllerRef.PlayerCamera.transform.right * 0.5f;
        transformPosition += _playerControllerRef.PlayerCamera.transform.up * -0.8f;
        transform.position = transformPosition;
        _renderer.material = redTransparentMaterial;
    }

    private bool IsOutOfRange(Vector3 placementPosition, Vector3 playerPosition)
    {
        var playerPositionOnPlane = new Vector3(playerPosition.x, placementPosition.y, playerPosition.z);
        var distance = Vector3.Distance(placementPosition, playerPositionOnPlane);
        return distance > _maxPlacementDistance;
    }

    private Vector3 CalcualtePlacementPosition(PlayerPlaceableLookAtData playerPlaceableLookAtData)
    {
        var vector = playerPlaceableLookAtData.staticObjectIntersectPosition;
        vector += playerPlaceableLookAtData.IntersectNormalVec * 0.55f;
        vector.y = 0.55f;
        return vector;
    }

    private bool IsSpawnPointColliding(bool recursiveCall = false)
    {
        var hits = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.gameObject.isStatic)
            {
                if (recursiveCall)
                {
                    Debug.LogError(
                        $"[SPAWN COLLISION RECURSION] Unexpected static object hit during position correction!\n" +
                        $"Current Position: {transform.position}\n" +
                        $"Hit Object: {hit.name}\n" +
                        $"Hit Object Tag: {hit.tag}\n" +
                        $"Hit Object Layer: {LayerMask.LayerToName(hit.gameObject.layer)}\n" +
                        $"Hit Collider Type: {hit.GetType().Name}\n" +
                        $"Hit Bounds: {hit.bounds}\n" +
                        $"Hit Transform Position: {hit.transform.position}\n" +
                        $"Distance to Hit: {Vector3.Distance(transform.position, hit.transform.position):F3}\n" +
                        $"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n" +
                        $"GameObject: {gameObject.name} (Tag: {tag})\n" +
                        $"Timestamp: {System.DateTime.Now:HH:mm:ss.fff}");
                    return true;
                }

                var moveVec = transform.position - hit.ClosestPoint(transform.position);
                if (moveVec.z > 0) moveVec.z = 0.55f - moveVec.z;
                if (moveVec.x > 0) moveVec.x = 0.55f - moveVec.x;
                if (moveVec.z < 0) moveVec.z = -0.55f - moveVec.z;
                if (moveVec.x < 0) moveVec.x = -0.55f - moveVec.x;
                transform.position += moveVec;
                return IsSpawnPointColliding(true);
            }

            return true;
        }

        return false;
    }

    public void Init(GameObject spawnPlayerRef)
    {
        var gs = GameState.Instance;
        _maxPlacementDistance = (int)(gs.CellSize * 1.5);
            
        _playerRef = spawnPlayerRef;

        _playerControllerRef = _playerRef.GetComponent<PlayerController>();

        _playerInput = _playerRef.GetComponent<PlayerInput>();
        _placeBombAction = _playerInput.actions["PlaceBomb"];

        _renderer = GetComponentInChildren<Renderer>();
        _defaultMaterial = _renderer.material;
        _isInitialized = true;
    }

    void PlaceBomb()
    {
        var hits = Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        foreach (var hit in hits)
        {
            if (hit.gameObject.isStatic) continue;
            if (hit.gameObject == gameObject) continue;
            return;
        }

        var go = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        var no = go.GetComponent<NetworkObject>();
        no.Spawn();
        
        Destroy(gameObject);
    }
}