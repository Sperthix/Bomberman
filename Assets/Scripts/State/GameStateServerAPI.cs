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
    public void PlaceBombServerRpc(Vector3 position, RpcParams rpcParams = default)
    {
        var hits = Physics.OverlapBox(position, new Vector3(0.5f, 0.5f, 0.5f));
        if (hits.Length > 0) return;
        // todo validate position further - distance from player on server side
        
        var go = Instantiate(bombPrefab, position, Quaternion.identity);
        var no = go.GetComponent<NetworkObject>();
        no.Spawn(true);
        
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ClientPlayerSpawnedServerRpc(RpcParams rpcParams = default)
    {
      GameStateManager.Instance.RegisterLoadedClientPlayer(rpcParams.Receive.SenderClientId);
    }




}