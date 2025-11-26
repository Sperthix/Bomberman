using System;
using UnityEngine;
using System.Collections;
using State;

public class BombExplode : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private int range = 2;

    private GameState _gs;

    private void OnEnable()
    {
        _gs = GameState.Instance;
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        var playerGridVec = _gs.WorldToGrid(_gs.PlayerRef.transform.position);
        var bombGridVec = _gs.WorldToGrid(transform.position);


        Vector2Int[] dirs =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down,
        };

        foreach (var dir in dirs)
        {
            ExplodeInLine(bombGridVec, dir, range, playerGridVec);
        }

        ExplodeInLine(bombGridVec, Vector2Int.zero, 1, playerGridVec);


        Destroy(gameObject);
    }

    private void ExplodeInLine(Vector2Int explodeGridOrigin, Vector2Int dir, int rangeInDir, Vector2Int playerGridVec)
    {
        for (var step = 1; step <= rangeInDir; step++)
        {
            Vector2Int tileGridVec = explodeGridOrigin + dir * step;

            GridTile tile;
            try
            {
                tile = _gs.WallMap[tileGridVec.x, tileGridVec.y];
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log(e);
                return;
            }

            switch (tile.Type)
            {
                case WallType.WallIndestructible:
                    return;
                case WallType.WallDestructible:
                    var wallBehaviour = _gs.WallObjects[tileGridVec.x, tileGridVec.y];
                    if (wallBehaviour)
                    {
                        wallBehaviour.HitByExplosion();
                    }

                    return;
                case WallType.Empty:
                    // spawn vfx

                    if (tileGridVec == playerGridVec)
                    {
                        _gs.PlayerRef.GetComponent<PlayerHealth>().TakeDamage(1);
                    }

                    break;
                default:
                    Console.WriteLine($"Unknown tile type: {tile.Type}");
                    break;
            }
        }
    }
}