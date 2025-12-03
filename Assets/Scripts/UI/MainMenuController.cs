using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private VisualTreeAsset mainMenuScreenAsset;
        [SerializeField] private VisualTreeAsset settingsScreenAsset;
        [SerializeField] private VisualTreeAsset gameModeSelectionScreenAsset;
        [SerializeField] private VisualTreeAsset multiplayerModeScreenAsset;

        private VisualElement screenContainer;
        private VisualElement currentScreen;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;
            
            screenContainer = root.Q<VisualElement>("screen-container");

            ShowMainMenu();
        }

        private void ShowScreen(VisualTreeAsset asset, System.Action<VisualElement> wireAction)
        {
            if (!asset)
            {
                Debug.LogError("MainMenuController: VisualTreeAsset is not set");
                return;
            }

            screenContainer.Clear();

            currentScreen = asset.CloneTree();
            screenContainer.Add(currentScreen);

            wireAction?.Invoke(currentScreen);
        }
        

        private void ShowMainMenu()
        {
            ShowScreen(mainMenuScreenAsset, WireMainMenu);
        }

        private void ShowSettings()
        {
            ShowScreen(settingsScreenAsset, WireSettings);
        }

        private void ShowGameModeSelection()
        {
            ShowScreen(gameModeSelectionScreenAsset, WireGameModeSelection);
        }

        private void ShowMultiplayerSelection()
        {
            ShowScreen(multiplayerModeScreenAsset, WireMultiplayerSelection);
        }

        
        private void WireMainMenu(VisualElement screenRoot)
        {
            var btnPlay     = screenRoot.Q<Button>("btn-play");
            var btnSettings = screenRoot.Q<Button>("btn-settings");
            var btnQuit     = screenRoot.Q<Button>("btn-quit");
            
            btnPlay.clicked += ShowGameModeSelection;
            btnSettings.clicked += ShowSettings;
            btnQuit.clicked += Application.Quit;
        }

        private void WireSettings(VisualElement screenRoot)
        {
            var btnBack = screenRoot.Q<Button>("btn-back");
            btnBack.clicked += ShowMainMenu;
        }

        private void WireGameModeSelection(VisualElement screenRoot)
        {
            var btnSp   = screenRoot.Q<Button>("gamemode-selection-sp");
            var btnMp   = screenRoot.Q<Button>("gamemode-selection-mp");
            var btnBack = screenRoot.Q<Button>("gamemode-selection-back");

            btnSp.clicked += () => GameManager.Instance.StartSinglePlayerGame();
            btnMp.clicked += ShowMultiplayerSelection;
            btnBack.clicked += ShowMainMenu;
        }

        private void WireMultiplayerSelection(VisualElement screenRoot)
        {
            var btnHost = screenRoot.Q<Button>("mp-host-btn");
            var btnJoin = screenRoot.Q<Button>("mp-join-btn");
            var btnBack = screenRoot.Q<Button>("mp-back-btn");

            btnHost.clicked += () =>
            {
                Debug.Log("Multiplayer: Host selected");
                // TODO: MP host game
            };

            btnJoin.clicked += () =>
            {
                Debug.Log("Multiplayer: Join selected");
                // TODO: MP join game
            };

            btnBack.clicked += ShowGameModeSelection;
        }
    }
}
