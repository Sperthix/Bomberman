using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class EndGameUIManager : MonoBehaviour
{
    public static EndGameUIManager Instance { get; private set; }

    private Button quitButton;
    private Button PlayAgainButton;

    private PostGameManager postGameManager;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;

        quitButton = root.Q<Button>("quit");
        PlayAgainButton = root.Q<Button>("play-again");
        postGameManager = PostGameManager.Instance;
        BindUI();
        UpdatePlayAgainButtonText();
    }

    private void BindUI()
    {
        postGameManager.PlayersVotedToRestart.OnListChanged += HandlePlayersVotedToRestartChanged;
        PlayAgainButton.clicked += PlayAgainButton_clicked;
    }

    private void PlayAgainButton_clicked()
    {
        postGameManager.RequestPlayAgainRpc();
    }

    private void OnDestroy()
    {
        postGameManager.PlayersVotedToRestart.OnListChanged -= HandlePlayersVotedToRestartChanged;
    }

    private void HandlePlayersVotedToRestartChanged(NetworkListEvent<ulong> changeEvent)
    {
        UpdatePlayAgainButtonText();
    }

    private void UpdatePlayAgainButtonText()
    {
        int totalPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int votes = postGameManager.PlayersVotedToRestart.Count;
        PlayAgainButton.text = $"Play Again ({votes}/{totalPlayers})";
    }
}