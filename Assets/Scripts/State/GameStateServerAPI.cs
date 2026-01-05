using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameStateServerAPI : NetworkBehaviour
{
    public static GameStateServerAPI Instance { get; private set; }
    [SerializeField] private GameObject bombPrefab;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PlaceBombServerRpc(Vector3 position, uint prefabUID, RpcParams rpcParams = default)
    {
        var bombPrefabGO = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs
            .First(p => p.SourcePrefabGlobalObjectIdHash == prefabUID).Prefab;
        var renderer = bombPrefabGO.GetComponentInChildren<Renderer>();
        Vector3 size = renderer.bounds.size; 
            
        var hits = Physics.OverlapBox(position, size/2);
        if (hits.Length > 0) return;
        // todo validate position further - distance from player on server side


        var go = Instantiate(bombPrefabGO, position, Quaternion.identity);
        var no = go.GetComponent<NetworkObject>();
        no.Spawn(true);
        go.GetComponent<BombExplode>().bombOwnerPlayerUid.Value = rpcParams.Receive.SenderClientId;
        
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ClientPlayerSpawnedServerRpc(RpcParams rpcParams = default)
    {
      GameStateManager.Instance.RegisterLoadedClientPlayer(rpcParams.Receive.SenderClientId);
    }




}