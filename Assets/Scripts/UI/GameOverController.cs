using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class GameOverController : MonoBehaviour
    {
        private VisualElement gameOverScreen;
        private Button btnRestart;
        private Button btnMainMenu;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;

            gameOverScreen = root.Q<VisualElement>("game-over-screen");
            gameOverScreen.style.display = DisplayStyle.None;

            btnRestart = root.Q<Button>("btn-restart");
            btnMainMenu = root.Q<Button>("btn-main-menu");
            btnRestart.clicked += OnRestartClicked;
            btnMainMenu.clicked += OnMainMenuClicked;
            
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.GameOver:
                    ToggleGameOver(true);
                    break;
                case GamePhase.Playing:
                case GamePhase.MainMenu:
                default:
                    ToggleGameOver(false);
                    break;
            }
        }

        private void ToggleGameOver(bool show)
        {
            if (show)
            {
                gameOverScreen.style.display = DisplayStyle.Flex;
                return;
            }
            gameOverScreen.style.display = DisplayStyle.None;
        }

        private static void OnRestartClicked()
        {
            GameManager.Instance.RestartGame();
        }

        private static void OnMainMenuClicked()
        {
            GameManager.Instance.BackToMainMenu();
        }
    }
}
