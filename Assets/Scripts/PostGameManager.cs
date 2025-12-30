using Unity.Netcode;
using UnityEngine;

public class PostGameManager : NetworkBehaviour
{
    public static PostGameManager Instance { get; private set; }

    public NetworkList<ulong> PlayersVotedToRestart;
    public NetworkList<ulong> PlayersToVote;
    private NetworkManager _networkManager;
    

    private void Awake()
    {
        // Singleton enforcement
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _networkManager = NetworkManager.Singleton;
        PlayersVotedToRestart = new NetworkList<ulong>();
        PlayersToVote = new NetworkList<ulong>(_networkManager.ConnectedClientsIds);
        GameManager.Instance.GameOverPhase();
        NetworkManager.Singleton.OnConnectionEvent += OnConnection;
    }

  
    
    private void OnConnection(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        Debug.Log("connection event " + connectionEventData.EventType);
        Debug.Log(connectionEventData.ClientId);

        if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            // server stopped
            if (connectionEventData.ClientId == _networkManager.LocalClientId)
            {
                GameManager.Instance.BackToMainMenu();
            }
            
            if (IsServer && PlayersToVote.Contains(connectionEventData.ClientId)) 
            {
                PlayersToVote.Remove(connectionEventData.ClientId);
                PlayersVotedToRestart.Remove(connectionEventData.ClientId);
                PlayersToVote.Remove(connectionEventData.ClientId);
            }
        }
    }
    
    [Rpc(SendTo.Server)]
    public void RequestPlayAgainRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (PlayersVotedToRestart.Contains(clientId)) return;

        PlayersVotedToRestart.Add(clientId);
        PlayersToVote.Remove(clientId);

        if (PlayersVotedToRestart.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            RestartGameClientRpc();
        }
    }
    
    [ClientRpc]
    public void RestartGameClientRpc()
    {
        GameManager.Instance.StartMultiPlayerGame(NetworkManager.Singleton.IsHost);
    }
}