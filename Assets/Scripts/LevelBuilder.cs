using System.Linq;
using State;
using Unity.Netcode;
using UnityEngine;

public class LevelBuilder : NetworkBehaviour
{
    [Header("Prefabs")] [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallIndestructiblePrefab;
    [SerializeField] private GameObject wallDestructiblePrefab;

    private GameState _state;

    public void ParseMapData(string mapData)
    {
        _state = GameState.Instance;

        var lines = mapData.Split('\n');
        var validLines = lines
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        _state.ArenaHeight = validLines.Count;
        _state.ArenaWidth = validLines[0].Length;

        if (validLines.Any(l => l.Length != _state.ArenaWidth))
        {
            Debug.LogError("Map data is inconsistent");
            return;
        }

        _state.Grid = new GridTile[_state.ArenaWidth, _state.ArenaHeight];


        for (var y = 0; y < _state.ArenaHeight; y++)
        {
            var line = validLines[y];

            for (var x = 0; x < _state.ArenaWidth; x++)
            {
                var c = line[x];

                switch (c)
                {
                    case 'X':
                        _state.Grid[x, y] = new GridTile(WallType.WallIndestructible);
                        break;

                    case 'W':
                        _state.Grid[x, y] = new GridTile(WallType.WallDestructible);
                        break;

                    case 'P':
                        _state.PlayerSpawns.Add(new PlayerSpawn(x, y));
                        _state.Grid[x, y] = new GridTile(WallType.Empty);
                        break;

                    case 'O':
                    case ' ':
                        _state.Grid[x, y] = new GridTile(WallType.Empty);
                        break;

                    default:
                        Debug.LogWarning($"Neznámy znak '{c}' na pozícii [{x},{y}]");
                        _state.Grid[x, y] = new GridTile(WallType.Empty);
                        break;
                }
            }
        }

        BuildLevel();
    }


    public void BuildLevel()
    {
        float cellSize = GameState.CellSize;

        for (int y = 0; y < _state.ArenaHeight; y++)
        {
            for (int x = 0; x < _state.ArenaWidth; x++)
            {
                GridTile tile = _state.Grid[x, y];

                Vector3 worldPos = new Vector3(x * cellSize, 0f, y * cellSize);

                switch (tile.Type)
                {
                    case WallType.WallIndestructible:
                        SpawnWallIndestructible(worldPos, x, y);
                        break;

                    case WallType.WallDestructible:
                        SpawnWallDestructible(worldPos, x, y);
                        break;

                    case WallType.Empty:
                        break;
                }
            }
        }
    }

    private void SpawnWallIndestructible(Vector3 pos, int x, int y)
    {
        if (wallIndestructiblePrefab == null) return;

        Vector3 p = pos + Vector3.up * 1f;
        GameObject go = Instantiate(wallIndestructiblePrefab, p, Quaternion.identity, transform);
        var no = go.GetComponent<NetworkObject>();
        no.Spawn(true);

        WallBehaviour wb = go.GetComponent<WallBehaviour>() ?? go.GetComponentInChildren<WallBehaviour>();
        if (wb != null)
        {
            wb.Init(x, y, WallType.WallIndestructible);
        }
        else
        {
            Debug.LogWarning($"Indestructible wall prefab '{go.name}' has no WallBehaviour script");
        }
    }

    private void SpawnWallDestructible(Vector3 pos, int x, int y)
    {
        if (wallDestructiblePrefab == null) return;

        Vector3 p = pos + Vector3.up * 1f;
        GameObject go = Instantiate(wallDestructiblePrefab, p, Quaternion.identity, transform);
        var no = go.GetComponent<NetworkObject>();
        no.Spawn(true);

        WallBehaviour wb = go.GetComponent<WallBehaviour>() ?? go.GetComponentInChildren<WallBehaviour>();
        if (wb != null)
        {
            wb.Init(x, y, WallType.WallDestructible);
        }
        else
        {
            Debug.LogWarning($"Destructible wall prefab '{go.name}' has no WallBehaviour script");
        }
    }
}