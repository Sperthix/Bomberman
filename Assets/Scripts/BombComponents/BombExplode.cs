using System;
using UnityEngine;
using System.Collections;
using DefaultNamespace;
using State;
using Unity.Netcode;

public class BombExplode : NetworkBehaviour, IExplosive
{
    [SerializeField] private int range = 2;
    
    public NetworkVariable<ulong> bombOwnerPlayerUid = new NetworkVariable<ulong>();
    
    
    public GameObject explosionVFXPrefab;
    
    
    private AudioSource _audioSource;
    private GameStateManager gs;
    
    private bool isExploded = false;
    
    private void Start()
    {
        
        gs = GameStateManager.Instance;
        _audioSource = GetComponent<AudioSource>();
        if (IsServer)
        {
            gs.RegisterDynamicGameObject(gameObject);
        }
    }
    
    
    public void OnExplosion()
    {
        Explode();
    }

    private void Explode()
    {
        if (isExploded) return;
        isExploded = true;
        
        PlaySoundExplosionPositionClientRpc(transform.position, 10f);
     
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
            ExplodeInLine(bombGridVec, dir, range);
        }

        ExplodeInLine(bombGridVec, Vector2Int.zero, 1);

        StartCoroutine(DespawnAfterSeconds(1f));

    }
    
    public void ExplodeInLine(Vector2Int explodeGridOrigin, Vector2Int dir, int rangeInDir)
    {
        var gameObjectsWithTilePlacement = gs.GetDynamicGameObjectsWithTilePlacement();
        
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
                    StartCoroutine(DelayedSpawnExplosionVFX(tileGridVec, dir, step * 0.05f));

                    
                    foreach (var dynamicGameObject in gameObjectsWithTilePlacement)
                    {
                        if (tileGridVec != dynamicGameObject.Value) continue;
                        
                        if (dynamicGameObject.Key.TryGetComponent<IExplosive>(out var explosive))
                        {
                            explosive.OnExplosion();
                        }

                        if (dynamicGameObject.Key.CompareTag("Player"))
                        {
                            dynamicGameObject.Key.GetComponent<PlayerHealth>().TakeDamage(1);
                        }

                    }
                    break;

                default:
                    Console.WriteLine($"Unknown tile type: {tile.Type}");
                    break;
            }
        }
    }

    private IEnumerator DelayedSpawnExplosionVFX( Vector2Int tileGridVec, Vector2Int directionVec, float delay)
    {
        yield return new WaitForSeconds(delay);
        var position = GridUtils.GridToWorld(tileGridVec.x, tileGridVec.y);
        position.y += 1f;
        SpawnExplosionVfxClientRpc(position, directionVec);
    }

    private IEnumerator DespawnAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gs.UnRegisterDynamicGameObject(gameObject);
        gameObject.GetComponent<NetworkObject>().Despawn(true);
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
    private void PlaySoundExplosionPositionClientRpc( Vector3 position, float volume = 1f)
    {
        
        AudioSource.PlayClipAtPoint(_audioSource.clip, position, volume);
    }



}