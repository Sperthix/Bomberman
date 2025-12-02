using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private VisualTreeAsset mainMenuScreenAsset;
        [SerializeField] private VisualTreeAsset settingsScreenAsset;
        
        private VisualElement screenContainer;
        private VisualElement currentScreen;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;

            screenContainer = root.Q<VisualElement>("screen-container");

            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            Debug.Log(mainMenuScreenAsset);
            Debug.Log(screenContainer);
            LoadScreen(mainMenuScreenAsset, wireMainMenu: true);
        }

        private void ShowSettings()
        {
            LoadScreen(settingsScreenAsset, wireSettings: true);
        }

        private void StartGame()
        {
            GameManager.Instance.StartLocalGame();
        }

        private void LoadScreen(VisualTreeAsset asset, bool wireMainMenu = false, bool wireSettings = false)
        {
            screenContainer.Clear();

            currentScreen = asset.CloneTree();
            screenContainer.Add(currentScreen);

            if (wireMainMenu)
            {
                WireMainMenu(currentScreen);
            }

            if (wireSettings)
            {
                WireSettings(currentScreen);
            }
        }

        private void WireMainMenu(VisualElement screenRoot)
        {
            var btnPlay = screenRoot.Q<Button>("btn-play");
            var btnSettings = screenRoot.Q<Button>("btn-settings");
            var btnQuit = screenRoot.Q<Button>("btn-quit");

            btnPlay.clicked += StartGame;
            btnSettings.clicked += ShowSettings;
            btnQuit.clicked += Application.Quit;
        }

        private void WireSettings(VisualElement screenRoot)
        {
            var btnBack = screenRoot.Q<Button>("btn-back");
            btnBack.clicked += ShowMainMenu;
        }
    }
}
