using System;
using UnityEngine;
using System.Collections;
using State;

public class BombExplode : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private int range = 2;

    private GameState gs;

    private void OnEnable()
    {
        gs = GameState.Instance;
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        var playerGridVec = gs.WorldToGrid(gs.PlayerRef.transform.position);
        var bombGridVec = gs.WorldToGrid(transform.position);

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

            GridTile tile = gs.GetTile(tileGridVec);
            if (tile == null)
            {
                return;
            }

            switch (tile.Type)
            {
                case WallType.WallIndestructible:
                    return;

                case WallType.WallDestructible:
                    // TODO: upgradnuta bomba moze znicit aj viac po sebe iducich stien
                    if (tile.Wall)
                    {
                        tile.Wall.HitByExplosion();
                    }
                    else
                    {
                        tile.Type = WallType.Empty;
                    }
                    return;

                case WallType.Empty:
                    if (tileGridVec == playerGridVec)
                    {
                        gs.PlayerRef.GetComponent<PlayerHealth>().TakeDamage(1);
                    }

                    // TODO: spawn VFX
                    break;

                default:
                    Console.WriteLine($"Unknown tile type: {tile.Type}");
                    break;
            }
        }
    }
}
