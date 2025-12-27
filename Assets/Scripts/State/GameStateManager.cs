using System.Collections;
using System.Collections.Generic;
using State;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : NetworkBehaviour
{
    public static GameState Instance { get; private set; }
    public LevelBuilder LevelBuilder;

    public GridTile[,] Grid;
    public List<PlayerSpawn> PlayerSpawns = new List<PlayerSpawn>();

    [SerializeField] private float cellSize = 2f;
    public float CellSize => cellSize;

    public int ArenaWidth;
    public int ArenaHeight;

    private List<GameObject> _playersInGame = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    private int _playerSpawnIndex = 0;


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
        DontDestroyOnLoad(gameObject);

        if (!IsServer) return;
        Load(defaultMap);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayer(clientId);
    }

    public void restartToDefaultMap()
    {
        Load(defaultMap);
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


    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.z / cellSize));
    }

    public Vector3 GridToWorld(int gx, int gy)
    {
        return new Vector3(gx * cellSize, 0f, gy * cellSize);
    }

    public GridTile GetTile(int x, int y)
    {
        return !IsInsideGrid(x, y) ? null : Grid[x, y];
    }

    public GridTile GetTile(Vector2Int pos)
    {
        return GetTile(pos.x, pos.y);
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
        var spawn = PlayerSpawns[_playerSpawnIndex % PlayerSpawns.Count];
        _playerSpawnIndex++;
        var playerSpawnLoc = GridToWorld(spawn.X, spawn.Y);
        playerSpawnLoc.y += 1f;

        var player = Instantiate(playerPrefab, playerSpawnLoc, Quaternion.identity, transform);
        _playersInGame.Add(player);
        player.GetComponent<PlayerHealth>().PlayerDied += OnPlayerDied;

        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);

        StartCoroutine(SetPlayerSpawnPositionNextFrame(player, playerSpawnLoc));
    }

    private void OnPlayerDied(GameObject player)
    {
        if (!IsServer) return;

        _playersInGame.Remove(player);
        CheckEndGame();
    }


    private void CheckEndGame()
    {
        var minPlayerCount = 0;
        if (GameManager.Instance.isMultiplayer.Value)
        {
            minPlayerCount = 1;
        }

        if (_playersInGame.Count <= minPlayerCount)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameManager.EndGame, LoadSceneMode.Single);
        }
    }


    private IEnumerator SetPlayerSpawnPositionNextFrame(GameObject player, Vector3 position)
    {
        yield return null; // Wait one frame
        player.GetComponent<PlayerController>().spawnPosition.Value = position;
    }
}