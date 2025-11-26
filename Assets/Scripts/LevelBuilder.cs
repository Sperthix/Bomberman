using System.Linq;
using UnityEngine;
using State;

public class LevelBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallIndestructiblePrefab;
    [SerializeField] private GameObject wallDestructiblePrefab;
    [SerializeField] private GameObject playerPrefab;

    private GameState state;

    private void Start()
    {
        state = GameState.Instance;

        if (state == null)
        {
            Debug.LogError("GameState.Instance not found!");
            return;
        }

        BuildLevel();
    }

    private void BuildLevel()
    {
        float cellSize = state.CellSize;

        for (int y = 0; y < state.ArenaHeight; y++)
        {
            for (int x = 0; x < state.ArenaWidth; x++)
            {
                GridTile tile = state.WallMap[x, y];

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

        SpawnPlayer();
    }
    
    private void SpawnWallIndestructible(Vector3 pos, int x, int y)
    {
        if (wallIndestructiblePrefab == null) return;

        Vector3 p = pos + Vector3.up * 1f;
        GameObject go = Instantiate(wallIndestructiblePrefab, p, Quaternion.identity, transform);
        
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

    private void SpawnPlayer()
    {
        GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(player => Destroy(player));
        var spawn = state.PlayerSpawns.First();
        var playerSpawnLoc = state.GridToWorld(spawn.X, spawn.Y);
        playerSpawnLoc.y += 1f;
        state.PlayerRef = Instantiate(playerPrefab, playerSpawnLoc, Quaternion.identity);
    }
}
