using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    MainMenu,
    Playing,
    GameOver
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; set; }
    public NetworkVariable<bool> isMultiplayer = new NetworkVariable<bool>(false);

    public GamePhase Phase { get; set; } = GamePhase.MainMenu;
    private const String GameSceneName = "GameScene";
    public static String EndGame = "EndGame";
    private const String MenuSceneName = "MainMenuScene";

    public event Action<GamePhase> OnPhaseChanged;

    private void Start()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void SetPhase(GamePhase phase)
    {
        if (Phase == phase) return;
        Phase = phase;
        OnPhaseChanged?.Invoke(Phase);
    }
    
    public void StartSinglePlayerGame()
    {
        if (GameState.Instance)
        {
            GameState.Instance.restartToDefaultMap();
        }
        
        Time.timeScale = 1f;
        SetPhase(GamePhase.Playing);
        SceneManager.LoadScene(GameSceneName);
    }
    
    public void StartMultiPlayerGame(bool isHost)
    {
        
        Time.timeScale = 1f;
        SetPhase(GamePhase.Playing);
        if (isHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
            isMultiplayer.Value = true;
        }
    }
    

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SetPhase(GamePhase.MainMenu);
        SceneManager.LoadScene(MenuSceneName);
    }
    
    public void GameOverPhase()
    {
        SetPhase(GamePhase.GameOver);
        Time.timeScale = 0f;
    }
}