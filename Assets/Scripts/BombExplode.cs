using UnityEngine;
using System.Collections;
using System.Linq;
using State;

public class BombExplode : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private int range = 2;

    private void OnEnable()
    {
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    public void Explode()
    {
        var gs = GameState.Instance;
        if (!gs)
        {
            Debug.LogError("No GameState");
            Destroy(gameObject);
            return;
        }
        
        if (!gs.WorldToGrid(transform.position, out var bx, out var by))
        {
            Debug.LogWarning("Bomb is outside arena grid!");
            Destroy(gameObject);
            return;
        }
        
        var affectedTiles = new System.Collections.Generic.List<Vector2Int> { new Vector2Int(bx, by) };
        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),   // right
            new Vector2Int(-1, 0),  // left
            new Vector2Int(0, 1),   // up
            new Vector2Int(0, -1)   // down
        };

        foreach (var dir in dirs)
        {
            for (var step = 1; step <= range; step++)
            {
                var x = bx + dir.x * step;
                var y = by + dir.y * step;

                // Check ci ide na kraj areny, keby sme v buducnosti nemali indestructible na kraje mapy
                if (x < 0 || x >= gs.ArenaWidth || y < 0 || y >= gs.ArenaHeight)
                    break;

                var tile = gs.WallMap[x, y];
                
                if (tile.Type == WallType.WallIndestructible)
                {
                    break;
                }
                
                affectedTiles.Add(new Vector2Int(x, y));
                
                if (tile.Type == WallType.WallDestructible)
                {
                    break;
                }

                // Empty → pokračujeme ďalej
            }
        }
        
        foreach (var tilePos in affectedTiles)
        {
            var x = tilePos.x;
            var y = tilePos.y;

            var tile = gs.WallMap[x, y];

            if (tile.Type != WallType.WallDestructible) continue;
            var wallBehaviour = gs.WallObjects[x, y];
            if (wallBehaviour)
            {
                wallBehaviour.HitByExplosion();
            }
            else
            {
                gs.WallMap[x, y] = new GridTile(WallType.Empty);
            }
        }

        // TODO: PlayerHealth / TakeDamage

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO)
        {
            var playerPos = playerGO.transform.position;
            if (gs.WorldToGrid(playerPos, out var px, out var py))
            {
                if (affectedTiles.Any(tilePos => tilePos.x == px && tilePos.y == py))
                {
                    Debug.Log("PLAYER HIT BY EXPLOSION!");
                }
            }
        }
        
        Destroy(gameObject);
    }
}
