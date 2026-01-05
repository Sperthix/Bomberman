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
    
    public void StartGame(bool isMp)
    {
        isMultiplayer.Value = isMp;
        RestartGame();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SetPhase(GamePhase.Playing);
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
    }
    

    public void BackToMainMenu()
    {
        NetworkManager.Singleton.Shutdown(true);
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