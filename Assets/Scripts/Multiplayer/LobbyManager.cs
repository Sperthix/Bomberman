using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Multiplayer
{
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public NetworkList<LobbyPlayerData> Players = new NetworkList<LobbyPlayerData>();
        public NetworkVariable<bool> everyoneIsReady = new NetworkVariable<bool>(false);

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            foreach (var t in NetworkManager.Singleton.ConnectedClientsIds)
                AddPlayerIfMissing(t);

            Players.OnListChanged += _ => RecomputeEveryoneIsReady();
            RecomputeEveryoneIsReady();
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            Players.OnListChanged -= _ => RecomputeEveryoneIsReady();
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            AddPlayerIfMissing(clientId);
            RecomputeEveryoneIsReady();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            for (var i = Players.Count - 1; i >= 0; i--)
            {
                if (Players[i].ClientId != clientId) continue;
                Players.RemoveAt(i);
                break;
            }

            RecomputeEveryoneIsReady();
        }

        private void AddPlayerIfMissing(ulong clientId)
        {
            if (Players.Contains(new LobbyPlayerData(clientId))) return;
            Players.Add(new LobbyPlayerData(clientId, false));
        }

        private void RecomputeEveryoneIsReady()
        {
            if (!IsServer) return;
            for (var i = 0; i < Players.Count; i++)
            {
                if (Players[i].IsReady) continue;
                Debug.Log($"Player {i} is not ready.");
                everyoneIsReady.Value = false;
                return;
            }

            everyoneIsReady.Value = true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetReadyRpc(bool ready, RpcParams rpcParams = default)
        {
            var senderId = rpcParams.Receive.SenderClientId;

            for (var i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                if (p.ClientId != senderId) continue;

                p.IsReady = ready;
                Players[i] = p;
                RecomputeEveryoneIsReady();
                return;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void RequestStartGameRpc()
        {
            if (!IsServer) return;
            if (!everyoneIsReady.Value) return;
            GameManager.Instance.StartGame(true);
        }
    }
}
