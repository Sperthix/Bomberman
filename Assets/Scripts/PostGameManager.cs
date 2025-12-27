using Unity.Netcode;

public class PostGameManager : NetworkBehaviour
{
    public static PostGameManager Instance { get; private set; }

    public NetworkList<ulong> PlayersVotedToRestart;
        
    

    private void Awake()
    {
        // Singleton enforcement
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        PlayersVotedToRestart = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        
    }

    [Rpc(SendTo.Server)]
    public void RequestPlayAgainRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (PlayersVotedToRestart.Contains(clientId)) return;

        PlayersVotedToRestart.Add(clientId);

        if (PlayersVotedToRestart.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            // todo restart game
        }
    }
}