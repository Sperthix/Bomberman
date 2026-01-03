using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using State;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public LevelBuilder LevelBuilder;

    public GridTile[,] Grid;
    public List<PlayerSpawn> PlayerSpawns = new List<PlayerSpawn>();

    public int ArenaWidth;
    public int ArenaHeight;

    private List<GameObject> _dynamicGameObjects = new();
    private Dictionary<ulong, int> _playerSpawnMap = new();
    [SerializeField] private GameObject playerPrefab;
    private int _playerSpawnIndex;
    
    private NetworkManager _networkManager;

    

    
    private string defaultMap = @"
        XXXXXXXXXXXXXXXXXXXX
        XPOPWOWOOOWOWOOOWOOX
        XOXOXWXWWXWXWXWWXWOX
        XPWOOOWOWOOOWOWOOOWX
        XWXWXWXOXWXWXOXWXWXX
        XOWOWOOOWOWOOOWOWOOX
        XWXOXWXWXOXWXWXOXWXX
        XOOOWOOOWOOOWOOOWOOX
        XWXWXWWXWXWXWWXWXWOX
        XOWOOOWOWOOOWOWOOOWX
        XWXWXOXWXWXOXWXWXWXX
        XOWOWOOOWOWOOOWOWOOX
        XWXOXWXWXOXWXWXOXWXX
        XOOOWOOOWOOOWOOOWOOX
        XWXWXWWXWXWXWWXWXWOX
        XOWOOOWOWOOOWOWOOOWX
        XWXWXOXWXWXOXWXWXWXX
        XOWOWOOOWOWOOOWOWOOX
        XOOOWOOOWOOOWOOOWOOX
        XXXXXXXXXXXXXXXXXXXX
    ";

    private void Start()
    {
        
        if (!IsServer) return;


        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _networkManager = NetworkManager.Singleton;
        
        if (!IsServer) return;
        Load(defaultMap);
        _networkManager.OnClientConnectedCallback += OnClientConnected;
    }

    public void OnDestroy()
    {
        if (_networkManager)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    } 

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayer(clientId);
    }
    
    private void Load(string mapData)
    {
        if (!IsServer) return;

        LevelBuilder.ParseMapData(mapData);

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }
    
    public GridTile GetTile(Vector2Int pos)
    {
        return !IsInsideGrid(pos.x, pos.y) ? null : Grid[pos.x, pos.y];
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < ArenaWidth && y >= 0 && y < ArenaHeight;
    }

    public void RegisterWall(int x, int y, WallBehaviour wall)
    {
        if (!IsInsideGrid(x, y))
        {
            Debug.LogError($"RegisterWall called with invalid coordinates: ({x}, {y})");
            return;
        }

        var tile = Grid[x, y];
        if (tile == null)
        {
            tile = new GridTile(WallType.Empty);
            Grid[x, y] = tile;
        }

        if (tile.Wall && tile.Wall != wall)
        {
            Debug.LogWarning("RegisterWall: attempt to register wall on occupied tile");
            return;
        }

        tile.Wall = wall;
    }

    public void UnregisterWall(int x, int y, WallBehaviour wall)
    {
        if (!IsInsideGrid(x, y)) return;

        var tile = Grid[x, y];
        if (tile == null || tile.Wall != wall) return;
        tile.Type = WallType.Empty;
        tile.Wall = null;
    }


    private void SpawnPlayer(ulong clientId)
    {
        // later we should have lobby and assign spawns on players 
        _playerSpawnMap[clientId] = _playerSpawnIndex;
        _playerSpawnIndex++;
        
        var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, transform);
        player.GetComponent<PlayerHealth>().PlayerDied += OnPlayerDied;

        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
    }

    private void OnPlayerDied(GameObject player)
    {
        if (!IsServer) return;

        _dynamicGameObjects.Remove(player);
        
        CheckEndGame();
    }


    private void CheckEndGame()
    {
        var minPlayerCount = 0;
        if (GameManager.Instance.isMultiplayer.Value)
        {
            minPlayerCount = 1;
        }

        if (_dynamicGameObjects.Count(o=> o.CompareTag("Player")) <= minPlayerCount)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameManager.EndGame, LoadSceneMode.Single);
        }
    }

    
    public void RegisterLoadedClientPlayer(ulong clientId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        RegisterDynamicGameObject(player);

        var spawn = PlayerSpawns[_playerSpawnMap[clientId]];
        var playerSpawnLoc = GridUtils.GridToWorld(spawn.X, spawn.Y);
        playerSpawnLoc.y += 1f;
        player.GetComponent<PlayerController>().spawnPosition.Value = playerSpawnLoc;
    }

    public void RegisterDynamicGameObject(GameObject go)
    {
        _dynamicGameObjects.Add(go);
    }


    public void UnRegisterDynamicGameObject(GameObject go)
    {
        _dynamicGameObjects.Remove(go);
    }

    public Dictionary<GameObject, Vector2Int> GetDynamicGameObjectsWithTilePlacement()
    {
        var dict = new Dictionary<GameObject, Vector2Int>();
        foreach (GameObject dynamicGameObject in _dynamicGameObjects)
        {
            dict.Add(
                dynamicGameObject,
                GridUtils.WorldToGrid(dynamicGameObject.transform.position));
        }
        return dict;
    }
}