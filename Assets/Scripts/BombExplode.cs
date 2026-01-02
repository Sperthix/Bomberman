using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using State;
using Unity.Netcode;

public class BombExplode : NetworkBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private int range = 2;
    
    public GameObject explosionVFXPrefab;
    
    private AudioSource _audioSource;
    private GameStateManager gs;

    private void Start()
    {
        gs = GameStateManager.Instance;
        _audioSource = GetComponent<AudioSource>();
        if (IsServer)
        {
            StartCoroutine(FuseCoroutine());
        }
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        PlaySoundExplosionPositionClientRpc(transform.position, 10f);
        var playersGridVec = new Dictionary<GameObject, Vector2Int>();

        foreach (var networkClient in NetworkManager.Singleton.ConnectedClientsList)
        {
            playersGridVec.Add(
                networkClient.PlayerObject.gameObject,
                GridUtils.WorldToGrid(networkClient.PlayerObject.transform.position));
          
        }
        var bombGridVec = GridUtils.WorldToGrid(transform.position);

        Vector2Int[] dirs =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down,
        };

        foreach (var dir in dirs)
        {
            ExplodeInLine(bombGridVec, dir, range, playersGridVec);
        }

        ExplodeInLine(bombGridVec, Vector2Int.zero, 1, playersGridVec);

        StartCoroutine(DespawnAfterSeconds(1f));

        Destroy(gameObject,1f);
    }

    private IEnumerator DespawnAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.GetComponent<NetworkObject>().Despawn(true);
    }


    private void ExplodeInLine(Vector2Int explodeGridOrigin, Vector2Int dir, int rangeInDir, Dictionary<GameObject, Vector2Int> playersGridVec)
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
                    StartCoroutine(DelayedSpawnVFX(tileGridVec, dir, step * 0.05f));

                    foreach (var playerData in playersGridVec)
                    {
                        if (tileGridVec == playerData.Value)
                        {
                            playerData.Key.GetComponent<PlayerHealth>().TakeDamage(1);
                        }
                    }
                   
                    break;

                default:
                    Console.WriteLine($"Unknown tile type: {tile.Type}");
                    break;
            }
        }
    }

    private IEnumerator DelayedSpawnVFX(Vector2Int tileVec, Vector2Int directionVec, float delay)
    {
        yield return new WaitForSeconds(delay);
        var position = GridUtils.GridToWorld(tileVec.x, tileVec.y);
        position.y += 1f;
        SpawnExplosionVfxClientRpc(position, directionVec);
    }

    
    [ClientRpc]
    public void SpawnExplosionVfxClientRpc(Vector3 position, Vector2Int directionVec)
    {
        Vector3 direction3D = new Vector3(directionVec.x, 0, directionVec.y);
        var rotation = Quaternion.LookRotation(direction3D);

        var vfx = Instantiate(explosionVFXPrefab,
            position, rotation);
        Destroy(vfx, 1);
    }
    
    
    [ClientRpc]
    public void PlaySoundExplosionPositionClientRpc( Vector3 position, float volume = 1f)
    {
        
        AudioSource.PlayClipAtPoint(_audioSource.clip, position, volume);
    }

}