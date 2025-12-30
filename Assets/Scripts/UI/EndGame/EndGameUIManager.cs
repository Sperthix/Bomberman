using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class EndGameUIManager : MonoBehaviour
{
    public static EndGameUIManager Instance { get; private set; }

    private Button _quitButton;
    private Button _playAgainButton;

    private PostGameManager _postGameManager;


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

        _quitButton = root.Q<Button>("quit");
        _playAgainButton = root.Q<Button>("play-again");
        _postGameManager = PostGameManager.Instance;
        BindUI();
    }

    private void BindUI()
    {
        _postGameManager.PlayersToVote.OnListChanged += HandlePlayersVotedToRestartChanged;
        _playAgainButton.clicked += PlayAgainButton_clicked;
        _quitButton.clicked += QuitButton_clicked;
    }

    private void QuitButton_clicked()
    {
        GameManager.Instance.BackToMainMenu();
    }

    private void PlayAgainButton_clicked()
    {
        _postGameManager.RequestPlayAgainRpc();
    }

    private void OnDestroy()
    {
        _postGameManager.PlayersVotedToRestart.OnListChanged -= HandlePlayersVotedToRestartChanged;
    }

    private void HandlePlayersVotedToRestartChanged(NetworkListEvent<ulong> changeEvent)
    {
        UpdatePlayAgainButtonText();
    }

    private void UpdatePlayAgainButtonText()
    {
        int waitingToVote = _postGameManager.PlayersToVote.Count;
        int voted = _postGameManager.PlayersVotedToRestart.Count;
        _playAgainButton.text = $"Play Again {voted}/{waitingToVote + voted}";
    }


}