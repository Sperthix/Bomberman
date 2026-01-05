using Multiplayer;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class LobbyController : MonoBehaviour
    {
        private ScrollView playersList;
        private Button btnStart;
        private Button btnReady;
        private Button btnBack;
        private Label hintLabel;

        private bool isReady;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            playersList = root.Q<ScrollView>("players-list");
            btnStart = root.Q<Button>("btn-start");
            btnReady = root.Q<Button>("btn-ready");
            btnBack = root.Q<Button>("btn-back");
            hintLabel = root.Q<Label>("lobby-hint");

            btnBack.clicked += BackToMenu;
            btnReady.clicked += ToggleReady;
            btnStart.clicked += StartGame;

            StartCoroutine(WaitForLobbyManager());
        }

        private System.Collections.IEnumerator WaitForLobbyManager()
        {
            while (!LobbyManager.Instance || !LobbyManager.Instance.IsSpawned)
                yield return null;

            LobbyManager.Instance.Players.OnListChanged += _ => RebuildPlayers();
            LobbyManager.Instance.everyoneIsReady.OnValueChanged += (_, __) => RefreshButtonsAndHint();

            RebuildPlayers();
            RefreshButtonsAndHint();
        }

        private void RebuildPlayers()
        {
            playersList.Clear();

            var lm = LobbyManager.Instance;

            foreach (var p in lm.Players)
            {
                var row = new VisualElement();
                row.AddToClassList("player-row");

                var left = new Label($"Player ID: {p.ClientId}");
                left.AddToClassList("player-row-label");

                var right = new Label(p.IsReady ? "READY" : "NOT READY");
                right.AddToClassList("player-row-status");
                right.AddToClassList(p.IsReady ? "ready" : "not-ready");

                row.Add(left);
                row.Add(right);

                playersList.Add(row);
            }

            RefreshButtonsAndHint();
        }

        private void RefreshButtonsAndHint()
        {
            var isHost = NetworkManager.Singleton.IsHost;

            btnReady.text = isReady ? "Unready" : "Ready";

            var everyoneIsReady = LobbyManager.Instance.everyoneIsReady.Value;
            btnStart.SetEnabled(isHost && everyoneIsReady);

            hintLabel.text = isHost
                ? (everyoneIsReady ? "Everyone is ready. You can start." : "Waiting for players to be ready...")
                : "Waiting for host to start...";
        }

        private void ToggleReady()
        {
            isReady = !isReady;
            LobbyManager.Instance.SetReadyRpc(isReady);
            RefreshButtonsAndHint();
        }

        private void StartGame()
        {
            LobbyManager.Instance.RequestStartGameRpc();
        }

        private void BackToMenu()
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
