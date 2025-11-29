using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPlaceableValidator : MonoBehaviour
{
    
    private GameObject _playerRef;
    private PlayerController _playerControllerRef;
    public GameObject bombPrefab;
    
    private PlayerInput _playerInput;
    private InputAction _placeBombAction;
    
    private Material _defaultMaterial;
    public Material redTransparentMaterial;
    
    private Renderer _renderer;
    
    void Update()
    {
        PlayerPlaceableLookAtData playerPlaceableLookAtData = _playerControllerRef.placeableLookAtData;
        if (playerPlaceableLookAtData == null)
        {
            // todo nejak zobrazit ze kukas do prázdna
            return;
        }

        // todo add offset to place bomb from intersect object 
        transform.position = playerPlaceableLookAtData.staticObjectIntersectPosition;

        var isColliding = IsSpawnPointColliding();
        
        //todo chcek max distance from player
        
        if (isColliding)
        {
           _renderer.material = redTransparentMaterial;
            
        }
        else
        {
            _renderer.material = _defaultMaterial;
        }
        
        if (_placeBombAction.WasReleasedThisFrame())
        {
            if (!isColliding)
            {
                PlaceBomb();
            }
            Destroy(gameObject);
        }
        
    }

    private bool IsSpawnPointColliding()
    {
        var hits = Physics.OverlapBox(transform.position, new Vector3(0.5f,0.5f,0.5f));
        foreach (var hit in hits)
        {
            if (hit.gameObject.isStatic) continue;
            if (hit.gameObject == gameObject) continue;
            return true; 
        }

        return false;
    }

    public void Init(GameObject spawnPlayerRef)
    {
        _playerRef = spawnPlayerRef;
        
        _playerControllerRef = _playerRef.GetComponent<PlayerController>();
        
        _playerInput = _playerRef.GetComponent<PlayerInput>();
        _placeBombAction = _playerInput.actions["PlaceBomb"];
        
        _renderer = GetComponentInChildren<Renderer>();
        _defaultMaterial = _renderer.material;
    }
    
    void PlaceBomb()
    {
        var hits = Physics.OverlapBox(transform.position, new Vector3(0.5f,0.5f,0.5f));
        foreach (var hit in hits)
        {
            if (hit.gameObject.isStatic) continue;
            if (hit.gameObject == gameObject) continue;
            return;
        }
        
        Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
