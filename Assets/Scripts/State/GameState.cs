using System.Collections.Generic;
using System.Linq;
using State;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    public GridTile[,] WallMap { get; private set; }
    public WallBehaviour[,] WallObjects { get; private set; }
    public GameObject PlayerRef { get; set; }

    public List<PlayerSpawn> PlayerSpawns { get; private set; }
    [SerializeField] private float cellSize = 2f;
    public float CellSize => cellSize;
    public int ArenaWidth { get; set; }
    public int ArenaHeight { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Load(@"
            XXXXXXX
            XPOOOWX
            XOXWXWX
            XWWOWWX
            XXXWXXX
            XWWWWWX
            XXXXXXX
        ");
    }

    private void Load(string mapData)
    {
        var lines = mapData.Split('\n');
        var validLines = lines
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        ArenaHeight = validLines.Count;
        ArenaWidth = validLines[0].Length;
        
        if (validLines.Any(l => l.Length != ArenaWidth))
        {
            Debug.LogError("Map data is inconsistent");
            return;
        }
        
        WallMap = new GridTile[ArenaWidth, ArenaHeight];
        WallObjects = new WallBehaviour[ArenaWidth, ArenaHeight];
        PlayerSpawns = new List<PlayerSpawn>();
        
        for (var y = 0; y < ArenaHeight; y++)
        {
            var line = validLines[y];
            
            for (var x = 0; x < ArenaWidth; x++)
            {
                var c = line[x];

                switch (c)
                {
                    case 'X':
                        WallMap[x, y] = new GridTile(WallType.WallIndestructible);
                        break;

                    case 'W':
                        WallMap[x, y] = new GridTile(WallType.WallDestructible);
                        break;

                    case 'P':
                        PlayerSpawns.Add(new PlayerSpawn(x, y));
                        WallMap[x, y] = new GridTile(WallType.Empty);
                        break;

                    case 'O':
                    case ' ':
                        WallMap[x, y] = new GridTile(WallType.Empty);
                        break;

                    default:
                        Debug.LogWarning($"Neznámy znak '{c}' na pozícii [{x},{y}]");
                        WallMap[x, y] = new GridTile(WallType.Empty);
                        break;
                }
            }
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
    
    public void RegisterWall(int x, int y, WallBehaviour wall)
    {
        if (x < 0 || x >= ArenaWidth || y < 0 || y >= ArenaHeight)
        {
            Debug.LogError($"RegisterWall called with invalid coordinates: ({x}, {y})");
            return;
        }
        WallObjects[x, y] = wall;
    }

    public void UnregisterWall(int x, int y, WallBehaviour wall)
    {
        if (WallObjects[x, y] == wall)
        {
            WallObjects[x, y] = null;
        }
    }
}
